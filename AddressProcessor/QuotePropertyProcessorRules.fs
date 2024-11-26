namespace Softellect.AddressProcessor

open FSharp.Data
open FSharp.Data.CsvExtensions

open AddressTypes
open Extensions
open DynamicData
open Errors
open CSharpInterop
open Configuration
open DatabaseTypes
open QuotePropertyParser
open DataUpdater
open DataParsing

module QuotePropertyProcessorRules =
    let private toError e = e |> QuotePropertyErr |> Error

    /// Non-existent streets used for testing purposes.
    let excludedStreetNames =
        [
            "NONEXISTENT DR"
        ]
        |> Set.ofList


    let isNotExcluded s = if excludedStreetNames |> Set.contains s then None else Some s


    type QuotePropertyLoader =
        | PropertyRangeLoader of (int -> int -> Result<list<QuotePropertyDataDetailed>, ApError>)
        | AllPropertyLoader of (unit -> list<QuotePropertyDataDetailed>)


    type QuotePropertyProcessorData =
        {
            tryLoadSettings : unit -> Result<QuotePropertyProcessorSetting option, ApError>
            trySaveSettings : QuotePropertyProcessorSetting -> UnitResult
            propertyLoader : QuotePropertyLoader
            tryUpsertStreetZipAddOn : StreetZipInfo -> ApResult<MergeAction option>
            getMaxQuotePropertyId : unit -> ApResult<int>
            addressProjector : AddressProjector
            zipCodeCityUpdater : AsyncUpdater<unit, ZipCodeCityMap>
            streetZipDetailedMapUpdater : AsyncUpdater<StreetZipInfo, StreetZipDetailedMap>
            logInfo : string -> unit
            logError : string -> unit
        }


    let resetProcessing logInfo trySaveSettings =
        logInfo "Resetting settings..."

        let settings =
            {
                quotePropertyId = 0
            }

        match trySaveSettings settings with
        | Ok() -> Ok ({ QuotePropertyProcessorResult.defaultValue with reset = true }, None)
        | Error e -> Error e


    /// Processes raw QuotePropertyDataDetailed and returns Ok StreetCityStateZip or some Error.
    /// The city is validated to exist in a given zip code but the street is not validated to exist because it might be a new street!
    let processQuotePropertyData (d : QuotePropertyProcessorData) (q : QuotePropertyDataDetailed) =
        let toError f = f |> ProcessQuotePropertyDataErr |> toError

        match q.StreetLine |> processStreetLine |> isNotExcluded with
        | Some s1 ->
            let s = Street.tryCreate cleanStringParams (d.addressProjector.projectStreet s1, Some s1)
            let c = City.tryCreate cleanStringParams (d.addressProjector.projectCity q.City, Some q.City)
            let st = State.tryCreate q.State
            let z = ZipCode.tryCreate q.ZipCode

            match s, c, st, z with
            | Some street, Some city, Some state, Some zipCode ->
                match d.zipCodeCityUpdater.getContent() |> Map.tryFind zipCode with
                | Some cities ->
                    match cities |> List.tryFind (fun (st, c) -> st = state && c.value = city.value) with
                    | Some stateCity -> { streetCityState = { street = street; city = snd stateCity; state = fst stateCity}; zipCode = zipCode } |> Some |> Ok
                    | None -> (q.QuotePropertyId, city.value) |> InvalidCity |> toError
                | None -> (q.QuotePropertyId, zipCode.value) |> InvalidZipeCodeErr |> toError
            | _ -> q.QuotePropertyId |> MissingDataErr |> toError
        | _ -> Ok None


    let processBatch loader d i j =
        $"Processing EFQuoteProperties with QuotePropertyId from %i{i + 1} to %i{j}." |> d.logInfo
        match loader i j with
        | Ok q -> q |> List.map (processQuotePropertyData d) |> Ok
        | Error e -> Error e


    let processAll loader d =
        sprintf "Processing all QuoteProperties. This might take a while..." |> d.logInfo
        loader() |> List.map (processQuotePropertyData d)


    let processQuotePropertiesImpl (d : QuotePropertyProcessorData) (r : QuotePropertyProcessorRequest) =
        let addError f e = ((f |> ProcessQuotePropertyDataErr |> QuotePropertyErr) + e) |> Error
        let toError e = e |> ProcessQuotePropertyDataErr |> toError

        let processResults maxIdOpt f1 (x : list<StreetCityStateZip option>) f2 =
            x |> List.choose id |> List.map (fun e -> e.toStreetZipInfo() |> d.streetZipDetailedMapUpdater.updateContent) |> ignore
            let allStreets = d.streetZipDetailedMapUpdater.getContent()

            let f3 =
                match maxIdOpt with
                | Some maxId -> d.trySaveSettings { quotePropertyId = maxId }
                | None -> Ok()

            let (r, f4) =
                allStreets
                |> Map.toList
                |> List.map snd
                |> List.map d.tryUpsertStreetZipAddOn
                |> unzipResults

            let inserted = r |> List.filter (fun e -> e = Some InsertedRow)
            let allFailures = [ foldToUnitResult f4; f3; foldToUnitResult (f2 @ f1)] |> foldUnitResults |> toErrorOpt
            let errors = allFailures |> Option.map (fun e -> e.errorCount) |> Option.defaultValue 0

            let result =
                {
                    reset = false
                    allProcessed = x.Length + f2.Length = 0
                    processedProperties = x.Length + f2.Length
                    streets = allStreets.Count
                    newStreets = inserted.Length
                    errors = errors
                }

            Ok (result, allFailures)

        match d.tryLoadSettings() with
        | Ok (Some s) ->
            match d.propertyLoader with
            | PropertyRangeLoader loader ->
                match d.getMaxQuotePropertyId() with
                | Ok maxId ->
                    let fromId = s.quotePropertyId
                    let toId = min maxId (s.quotePropertyId + (max (r.numberToProcess - quotePropertyBatchSize) quotePropertyBatchSize))

                    if maxId > fromId
                    then
                        let batches = [ for i in fromId .. quotePropertyBatchSize .. toId -> (i, i + quotePropertyBatchSize) ]
                        let maxId = batches |> List.rev |> List.tryHead |> Option.bind (fun e -> e |> snd |> Some) |> Option.defaultValue s.quotePropertyId
                        let (w, f1) = batches |> List.map (fun e -> e ||> processBatch loader d) |> unzipResults
                        let (x, f2) = w |> List.concat |> unzipResults
                        processResults (Some maxId) f1 x f2
                    else Ok (QuotePropertyProcessorResult.allProcessedValue, None)
                | Error e -> addError UnableToLoadMaxQuotePropertyIdErr e
            | AllPropertyLoader loader -> processAll loader d |> unzipResults ||> processResults None []
        | Ok None -> toError UnableToLoadQuotePropertySettingsErr
        | Error e -> addError UnableToLoadQuotePropertySettingsErr e


    let processQuoteProperties (d : QuotePropertyProcessorData) (r : QuotePropertyProcessorRequest) =
        let result =
            match r.resetProcessing with
            | true -> resetProcessing d.logInfo d.trySaveSettings
            | false -> processQuotePropertiesImpl d r

        result


    let loadProperties logInfo fileName =
        $"Loading data from file: %s{fileName}" |> logInfo

        let data =
            CsvFile.Load(fileName, separators = "|").Cache().Rows
            |> Seq.map (fun r ->
                    {
                        QuotePropertyId = r?QuotePropertyId.AsInteger()
                        StreetLine = r?Street1
                        City = r?City
                        State = r?StateCode
                        ZipCode = r?Zip
                    })
            |> List.ofSeq

        data


    let getData (p : QuotePropertyProcessorParam) fo =
        let d = p.sqlConnectionData
        let zipCodeCityUpdater = zipCodeCityMapUpdater d.ratingConnectionGetter
        do zipCodeCityUpdater.updateContent()
        let streetZipDetailedMapUpdater = streetZipDetailedMapUpdater()

        {
            tryLoadSettings = fun() -> tryLoadQuotePropertyProcessorSetting d.ratingConnectionGetter
            trySaveSettings = tryUpsertQuotePropertyProcessorSetting d.ratingConnectionGetter

            propertyLoader =
                match fo with
                | None | Some "" -> loadQuotePropertyTbl d.coreConnectionGetter |> PropertyRangeLoader
                | Some fileName -> (fun () -> loadProperties p.logInfo fileName) |> AllPropertyLoader

            tryUpsertStreetZipAddOn = tryUpsertStreetZipAddOn d.ratingConnectionGetter
            getMaxQuotePropertyId = fun() -> getMaxQuotePropertyId d.coreConnectionGetter
            addressProjector = AddressProjector d
            zipCodeCityUpdater = zipCodeCityUpdater
            streetZipDetailedMapUpdater = streetZipDetailedMapUpdater
            logInfo = p.logInfo
            logError = p.logError
        }
