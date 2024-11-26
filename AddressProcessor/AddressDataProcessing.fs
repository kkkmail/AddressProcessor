namespace Softellect.AddressProcessor

open System
open System.IO
open FSharp.Collections.ParallelSeq
open StringParser
open AddressTypes
open Extensions
open DynamicData
open AddressDataDataTypes
open AddressDataRules
open AddressDataUtilities
open Configuration
open DynamicSql

open Microsoft.Data.SqlClient

module AddressDataProcessing =

    type AddressDataConfig =
        {
            getRatingConn : unit -> SqlConnection
            logError : (string -> unit)
            logInfo : (string -> unit)
            parallelRun : bool
            batchSize : int
            outputFile : string
        }


    type AddressDataProcessor (config : AddressDataConfig) =
        let connGetter = RatingConnectionGetter config.getRatingConn
        let logError = config.logError
        let logInfo = config.logInfo
        let logExn e = config.logError (e.ToString())
        let parallelRun = config.parallelRun
        let batchSize = config.batchSize

        let zipCodeCityMap = zipCodeCityMapUpdater connGetter
        let zipCodeCityMetricMap = zipCodeCityMetricMapUpdater connGetter
        let zipToCityMap = zipToCityMapUpdater connGetter
        let countyMap = countyMapUpdater connGetter
        let streetZipDetailedMapUpdater = streetZipDetailedMapUpdater()


        let updater =
            {
                zipCodeCityUpdater = zipCodeCityMap
                zipCodeCityMetricUpdater = zipCodeCityMetricMap
                zipToCityMapUpdater = zipToCityMap
                countyUpdater = countyMap
            }

        do updater.updateContent()


        let maxIdValue = (new DynamicSqlConnection(connGetter.getConnection)?<<addressDataMaxIdSql).ExecuteScalar() :?> int


        let toStreetZipInfo (r : AddressData) =
            {
                key =
                    {
                        StreetFullName = r.streetCityState.street.value
                        City = r.streetCityState.city.value
                        State = r.streetCityState.state
                        ZipCode = r.zipCode.value
                    }

                OccurrenceCount = r.occurrencCount
                StreetOriginalName = r.streetCityState.street.originalValue
                CityOriginalName = r.streetCityState.city.originalValue
            }


        let processBatch inputRowData =
            inputRowData |> List.map toStreetZipInfo |> List.map (fun e -> streetZipDetailedMapUpdater.updateContent e) |> ignore
            sprintf "processBatch::Batch prepared." |> logInfo


        let loadData startID endID =
            $"Running for startID = %A{startID}, endID = %A{endID}, start time = %A{DateTime.Now}" |> logInfo
            seq {
                use conn = new DynamicSqlConnection(connGetter.getConnection)
                use data = conn?<<addressDataSql
                data?StartID <- startID
                data?EndID <- endID
                use reader = data.ExecuteReader()

                while (reader.Read())
                    do yield
                        {|
                            Id = int reader?AddressDataId
                            City = string reader?City
                            State = string reader?StateCode
                            ZipCode = string reader?Zip
                            StreetFullName = string reader?StreetFullName
                            Source = string reader?Source
                            OccurrenceCount = int reader?OccurrenceCount
                        |} }
            |> List.ofSeq
            |> List.map (fun r ->
                        let zo = r.ZipCode |> ZipCode.tryCreate
                        let so = (r.StreetFullName, Some r.StreetFullName) |> Street.tryCreate cleanStringParams

                        let co =
                            match zo, (r.City, Some r.City) |> City.tryCreate cleanStringParams with
                            | Some v, Some city ->
                                match updater.zipCodeCityUpdater.getContent().TryFind v with
                                | Some x -> x |> List.map (fun (_, c) -> c) |> List.tryFind (fun c -> c.value = city.value)
                                | None -> None
                            | _ -> None

                        let sto = r.State |> State.tryCreate

                        match so, co, sto, zo with
                        | Some s, Some c, Some st, Some z ->
                            {
                                rowId = r.Id

                                streetCityState =
                                    {
                                        street = s
                                        city = c
                                        state = st
                                    }

                                zipCode = z
                                occurrencCount = r.OccurrenceCount
                                source =
                                    match r.Source with
                                    | "MD3" -> MD3
                                    | "MD4" -> MD4
                                    | "OA" -> OA
                                    | _ -> Unknown
                            }
                            |> Some
                        | _ -> None)
            |> List.choose id


        let truncateTables () =
            use conn = new DynamicSqlConnection(connGetter.getConnection)
            use data = conn?<<truncateStreetZipSql
            data.ExecuteNonQuery() |> ignore


        let updateStreetZipTbl r =
            let cmdText = "
                insert into dbo.EFStreetZips
                        (City
                        ,StateCode
                        ,Zip
                        ,OccurrenceCount
                        ,StreetFullName
                        ,StreetOriginalName)
                     values
                        (@City
                        ,@StateCode
                        ,@Zip
                        ,@OccurrenceCount
                        ,@StreetFullName
                        ,@StreetOriginalName)"

            use conn = new DynamicSqlConnection(connGetter.getConnection)
            use data = conn?<<cmdText
            data?City <- r.key.City
            data?StateCode <- r.key.State.key
            data?Zip <- r.key.ZipCode
            data?OccurrenceCount <- r.OccurrenceCount
            data?StreetFullName <- r.key.StreetFullName
            data?StreetOriginalName <- r.StreetOriginalName
            data.ExecuteNonQuery() |> ignore


        let saveStreetZip () =
            printfn "Saving StreetZip ..."
            let data = streetZipDetailedMapUpdater.getContent() |> Map.toList |> List.map (fun (_, b) -> b)
            data |> List.map updateStreetZipTbl |> ignore
            printfn $"... saved %A{data.Length} rows."


        let processAddressDataImpl maxVal loader =
            let start = DateTime.Now

            // Don't care about the extra tail.
            let noOfBatches = 2 + maxVal / batchSize

            let runBatch i =
                let startID = ((i - 1) * batchSize)
                let endID = startID + batchSize
                $"Start time = %A{start}. Running batch %A{i} of %A{noOfBatches}..." |> logInfo
                loader startID endID
                |> processBatch

                let now = DateTime.Now
                let diff = now - start
                let speed = diff.DivideBy (int64 i)
                let estEnd = now + (speed.MultiplyBy (int64 (noOfBatches - i)))
                $"Batch run time: %A{speed}, estimated end of work: %A{estEnd}" |> logInfo

            let batches = [| for i in 1..noOfBatches -> i |]

            if parallelRun
            then
                batches
                |> PSeq.map runBatch
                |> PSeq.iter id
                |> ignore
            else
                batches
                |> Array.map runBatch
                |> ignore

            $"Finished. Total run time: %A{(DateTime.Now - start).Minutes} minutes." |> logInfo


        let saveErrors() =
            try
                match config.outputFile with
                | EmptyString -> printfn "Output file was not specified."
                | _ ->

                    printfn $"Saving errors into file: '%s{config.outputFile}'"
                    let header = "Id"
                    let output = allErr.getContent() |> List.map (fun e -> e.ToString())
                    File.WriteAllLines(config.outputFile, header :: output)
            with
            | e -> printfn $"Exception occurred saving errors into file: '%s{config.outputFile}'. Exception:\n'%A{e}'"


        let processAll () =
            match maxIdValue with
            | 0 -> printfn "Table AddressData is empty. Please, run script: \Data\AddressProcessor\All\all.sql before running this task."
            | _ ->
                truncateTables ()
                processAddressDataImpl maxIdValue loadData
                saveStreetZip()
                saveErrors()


        member _.processAddressData() = processAll()
