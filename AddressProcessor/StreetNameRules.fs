namespace Softellect.AddressProcessor

open AddressTypes
open MatchTypes
open StringParser
open MatchingRules
open MatchParams
open MetricTreeInterop
open Extensions

module StreetNameRules =

    type StreetRuleMatchData =
        {
            wordMap : Map<string, string>
            prevMatch : Address -> MatchInfo -> MatchInfo
            tryFind : list<string> -> list<StreetCityState> option
            fuzzySearchParams : FuzzySearchParams
        }

        member d.toStreetValidationInputData c =
            {
                streetValidationCityOpt = c
                fuzzySearchParams = d.fuzzySearchParams
            }


    let streetFailed m = { m with result = Failed; matchError = Some StreetNotFound }
    let setStreetResult r m = { m with stepResults = { m.stepResults with streetResult = r } }


    let prevByZip a m =
        match a.zipCodeOpt = m.address.zipCodeOpt with
        | false -> streetFailed m
        | true -> { m with address = { m.address with streetOpt = a.streetOpt }; matchError = None } |> setStreetResult Inferred


    let prevByCityState a m =
        match a.cityOpt = m.address.cityOpt && a.stateOpt = m.address.stateOpt with
        | false -> streetFailed m
        | true -> { m with address = { m.address with streetOpt = a.streetOpt }; matchError = None } |> setStreetResult Inferred


    let cityValidator p a =
        match p.streetValidationCityOpt with
        | Some c ->
            match c = a.city with
            | true -> Perfect
            | false ->
                let x = c.value.normalizedDistance a.city.value
                Partial x
        | None -> Perfect


    /// TODO kk:20200324 - Delete after 30 days.
    let distanceValidatorOld (projector : string -> string) comparand (street : Street) =
        let b0 = street.toList
        let b = b0 |> String.concat SingleSpace |> projector
        let b1 = street.toOriginalList |> String.concat SingleSpace |> projector
        let c = comparand |> List.rev |> String.concat SingleSpace |> projector

        match b = c, b1 = c with
        | true, _ -> Perfect
        | _, true -> Perfect
        | _ ->
            let e = b.normalizedDistance c
            let e1 = b1.normalizedDistance c
            let r = min e e1
            Partial r


    let distanceValidator (projector : string -> string) comparand (street : Street) =
        let f a = a |> String.concat SingleSpace |> projector

        let project a =
            let rec inner tail acc =
                match tail with
                | [] -> acc |> List.distinct |> List.map List.rev |> List.map f
                | h :: t ->
                    let b = [ h; standardizeString CleanStringParams.defaultValue h ] |> List.distinct

                    match acc with
                    | [] -> b |> List.map (fun e -> [e])
                    | _ -> b |> List.allPairs acc |> List.map (fun (a, e) -> e :: a)
                    |> inner t

            inner a []

        let b0 = street.toList
        let b1 = street.toOriginalList
        let c = comparand |> List.rev
        let c1 = f c

        match [ b0, c; b1, c ] |> List.tryFind (fun (a, b) -> f a = f b) with
        | Some _ -> Perfect
        | None ->
            project b1
            |> List.map (fun e -> e.normalizedDistance c1)
            |> List.sort
            |> List.tryHead
            |> Option.map Partial
            |> Option.defaultValue Failed


    let compareListsValidator _ d a =
        let result = compareLists getWeight (a.street.toList |> List.rev) d.comparand
        result


    let normalizedDistanceValidator _ d a =
        let result = distanceValidator id d.comparand a.street
        result

    let normalizedDistWithProjectionValidator (p : StreetValidationInputData) d a =
        let result = distanceValidator p.fuzzySearchParams.streetProjector d.comparand a.street
        result


    type FuzzyBaseComparisionType
        with

        member f.streetValidator =
            match f with
            | CompareLists -> compareListsValidator
            | NormalizedDistance -> normalizedDistanceValidator
            | NormalizedDistWithProjection -> normalizedDistWithProjectionValidator


    /// Validates that a given street from a given StreetCityState (a) matches list of strings (d.comparand) chosen from a list of strings (d.fullComparand).
    /// City is used to constraint that cities must match as well.
    let validateStreetImpl (p : StreetValidationInputData) d a =
        let v, f =
            match p.fuzzySearchParams.streetComparisionType with
            | BestOf (a, b) -> a :: b, List.min
            | AverageOf (a, b) -> a :: b, List.average
            | AverageNotFailedOf (a, b) -> a :: b, averageOrDefault Failed

        let result =
            let r = v |> List.map (fun e -> e, (e.streetValidator p d a) + (cityValidator p a))
            r |> List.map snd |> f

        result


    let tryAfterStreet (ri, m) =
        (ri, m)


    let streetRuleImpl f t (ri, m) =
        let withStreetFailed() = failed (ri, streetFailed m)

        let getMatchData w p st =
            {
                wordMap = w
                prevMatch = p
                tryFind = st
                fuzzySearchParams = ri.matchParams.fuzzySearchParams
            }

        let retVal =
            match m.result, ri.rules with
            | Failed, _ -> failed (ri, m)
            | _ ->
                match m.unprocessed, m.address.zipCodeOpt, m.address.stateOpt, m.address.cityOpt, m.address.streetOpt with
                | [], _, _, _, None -> ri, m // No input to work - ignore.
                | _, None, None, None, _ -> withStreetFailed() // Must have zip OR (state + city) already resolved.
                | _, _, _, _, Some _ -> ri, m // Already has the street.
                | _ :: _, Some z, _, _, None ->
                    match ri.tryFind z with
                    | Some st -> (ri, t (getMatchData (ri.getWordMap z) prevByZip st) m) |> tryAfterStreet
                    | None -> failed (ri, f m) // Call a provided failure function (f) if cannot find any street for a given zip code.
                | _ :: _, _, Some s, Some c, None ->
                    match ri.tryFind (s, c) with
                    | Some st -> (ri, t (getMatchData Map.empty prevByCityState st) m) |> tryAfterStreet
                    | None -> withStreetFailed()
                | _ :: _, _, None, Some _, None -> withStreetFailed()
                | _ :: _, _, Some _, None, None -> withStreetFailed()

        retVal


    let tryInferStreet (d : StreetRuleMatchData) m = tryFromResolved streetFailed d.prevMatch m


    let streetTryInferRule (ri, m) =
        let retVal = streetRuleImpl streetFailed tryInferStreet (ri, m)
        retVal


    let validateStreet t =
        match t with
        | ExactSearch -> validateStreetImpl
        | FuzzySearch -> validateStreetImpl


    let tryMatchStreet (d : StreetRuleMatchData) m =
        let validateStreet = validateStreet d.fuzzySearchParams.streetSearchParams.streetSearchType

        match tryPickMatch d.wordMap d.tryFind m.unprocessed (validateStreet (d.toStreetValidationInputData m.address.cityOpt)) with
        | Some d, _ ->
            let u a =
                { a with
                    streetOpt = Some d.value.street;
                    cityOpt = getValueOrDefault d.value.city a.cityOpt |> Some
                    stateOpt = getValueOrDefault d.value.state a.stateOpt |> Some
                }

            updateMatchInfo d m u |> setStreetResult Matched
        | None, _ -> streetFailed m


    let streetTryMatchRule (ri, m) =
        let retVal = streetRuleImpl streetFailed tryMatchStreet (ri, m)
        retVal


    let streetRuleBase f (ri, m) =
        let c m = ri, streetFailed m

        let a =
            [
                streetTryInferRule
                streetTryMatchRule
            ]
            |> List.map(fun e -> e >> f)
            |> processRuleCollection c (defaultSorter ri.matchParams) (ri, m)

        a


    let trySetCity (ri, m) =
        match m.result, m.address.cityOpt with
        | Failed, _ -> (ri, m)
        | _, Some _ -> (ri, m)
        | _, None ->
            match m.address.streetCityStateOpt, m.address.zipCodeOpt with
            | Some w, Some z ->
                ri.mapData.citySelectMapUpdater.updateContent (w, z)

                match ri.mapData.citySelectMapUpdater.getContent() |> Map.tryFind (w, z) with
                | Some [ c ] ->
                    let address = { m.address with cityOpt = Some c }
                    ri, { m with address = address; stepResults = { m.stepResults with cityResult = Inferred } }
                | _ -> (ri, m)
            | _ -> (ri, m)


    let streetRule = streetRuleBase id
    let streetRuleWithTrySetCity = streetRuleBase trySetCity


    let streetSimpleRule (ri, m) =
        failwith "streetSimpleRule is not yet implemented."
