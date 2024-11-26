namespace Softellect.AddressProcessor

open AddressTypes
open MatchTypes
open MatchingRules
open MatchParams
open MetricTreeInterop

module CityRules =

    type CityRuleMatchData =
        {
            wordMap : Map<string, string>
            prevMatch : Address -> MatchInfo -> MatchInfo
            tryFindCity : list<string> -> list<City> option
            fuzzySearchParams : FuzzySearchParams
        }


    let cityFailed m = { m with result = Failed; matchError = Some CityNotFound }
    let setCityResult r m = { m with stepResults = { m.stepResults with cityResult = r } }
    let setZipResult r m = { m with stepResults = { m.stepResults with zipResult = r } }


    let prevByZip a m =
        match a.zipCodeOpt = m.address.zipCodeOpt with
        | false -> cityFailed m
        | true -> { m with address = { m.address with cityOpt = a.cityOpt }; matchError = None } |> setCityResult Inferred


    let prevByState a m =
        match a.stateOpt = m.address.stateOpt with
        | false -> cityFailed m
        | true -> { m with address = { m.address with cityOpt = a.cityOpt }; matchError = None } |> setCityResult Inferred


    let tryAfterCity (ri, m) =
        match m.result with
        | Failed -> failed (ri, m)
        | _ ->
            let a = (removeUnitNumber ri, Some m) :: [ for i in 0..ri.matchParams.cityRulesParams.maxUnitNumberSkip -> ri, trySkip i m ]
            let b = a |> processBoundRuleCollection (fun () -> failed (ri, cityFailed m)) (defaultSorter ri.matchParams)
            b


    let cityRuleImpl t (ri, m) =
        let withCityFailed () = failed (ri, cityFailed m)

        let getMatchData w p st =
            {
                wordMap = w
                prevMatch = p
                tryFindCity = st
                fuzzySearchParams = ri.matchParams.fuzzySearchParams
            }

        let f a =
            let b = tryAfterCity a
            b

        let retVal =
            match m.result, ri.rules with
            | Failed, _ -> failed (ri, m)
            | _ ->
                match m.unprocessed, m.address.zipCodeOpt, m.address.stateOpt, m.address.cityOpt with
                | [], _, _, _ -> ri, m // No input to work - ignore.
                | _, None, None, _ -> withCityFailed () // Must have zip OR state already resolved.
                | _, Some _, _, Some _ -> ri, m // Already have zip AND city.
                | _, _, Some _, Some _ -> ri, m // Already have state AND city.
                | _ :: _, Some z, _, None ->
                    match ri.tryFindCity z with
                    | Some st ->
                        let a = ri, t (getMatchData (ri.getWordMap z) prevByZip st) m
                        f a
                    | None -> withCityFailed ()
                | _ :: _, _, Some s, None ->
                    match ri.tryFindCity s with
                    | Some st ->
                        let a = (ri, t (getMatchData Map.empty prevByState st) m)
                        f a
                    | None -> withCityFailed ()

        retVal


    /// Validates that a given city (a) matches list of strings (b) chosen from list of strings (c).
    let validateCityFuzzy _ d (a : City) =
        let b = a.toList |> String.concat SingleSpace
        let b1 = a.toOriginalList |> String.concat SingleSpace
        let c = d.comparand |> List.rev |> String.concat SingleSpace

        match b = c, b1 = c with
        | true, _ -> Perfect
        | _, true -> Perfect
        | _ ->
            let e = b.normalizedDistance c
            let e1 = b1.normalizedDistance c
            let r = (min e e1)
            Partial r


    let validateCityExactly = validateCityFuzzy


    let validateCity t =
        match t with
        | ExactSearch -> validateCityExactly
        | FuzzySearch -> validateCityFuzzy


    let tryInferCity (d : CityRuleMatchData) m = tryFromResolved cityFailed d.prevMatch m


    /// Tries to infer city.
    let cityTryInferRule (ri, m) =
        let retVal = cityRuleImpl tryInferCity (ri, m)
        retVal


    let tryMatchCity (d : CityRuleMatchData) m =
        let validateCity = validateCity d.fuzzySearchParams.citySearchParams.citySearchType d

        match tryPickMatch d.wordMap d.tryFindCity m.unprocessed validateCity with
        | Some d, _ -> updateMatchInfo d m (fun a -> { a with cityOpt = Some d.value }) |> setCityResult Matched
        | None, _ -> cityFailed m


    /// Tries to match city.
    let cityTryMatchRule (ri, m) =
        let retVal = cityRuleImpl tryMatchCity (ri, m)
        retVal


    /// Tries to match city in a given zip code.
    let cityRule (ri, m) =
        let c m = ri, cityFailed m

        let a =
            [
                cityTryInferRule
                cityTryMatchRule
            ]

        let b = a |> processRuleCollection c (defaultSorter ri.matchParams) (ri, m)
        b


    /// Tries to set missing zip if resolution of state / city / street / ... was successful.
    let cityRuleWithTrySetZip (ri, m) =
        let r1, m1 = cityRule (ri, m)

        match m1.result, m1.resolved with
        | Failed, _ -> (r1, m1)
        | _, [] -> (r1, m1)
        | _, h :: t ->
            match h.resolvedAddress.streetCityStateOpt, h.resolvedAddress.zipCodeOpt with
            | Some w, None ->
                ri.mapData.zipCodeSelectMapUpdater.updateContent w

                match ri.mapData.zipCodeSelectMapUpdater.getContent() |> Map.tryFind w with
                | Some [ z ] ->
                    let h1 =
                        { h with
                            resolvedAddress = { h.resolvedAddress with zipCodeOpt = Some z }
                            resolvedStepResults = { h.resolvedStepResults with zipResult = Inferred }
                        }

                    (r1, { m1 with resolved = h1 :: t } |> setZipResult Inferred)
                | _ -> (r1, m1)
            | _ -> (r1, m1)


    /// City rule with no City.
    let cityRuleNoCity (ri, m) =
        let f a =
            let b = tryAfterCity a
            b

        let retVal =
            match m.result, ri.rules with
            | Failed, _ -> failed (ri, m)
            | _ ->
                match m.unprocessed, m.address.zipCodeOpt, m.address.stateOpt, m.address.cityOpt with
                | [], _, _, _ -> ri, m // No input to work - ignore.
                | _, Some _, _, Some _ -> ri, m // Already have zip AND city.
                | _, _, Some _, Some _ -> ri, m // Already have state AND city.
                | _ :: _, Some _, Some _, None -> f (ri, m)
                | _ -> failed (ri, cityFailed m) // Must have zip OR state already resolved.

        retVal


    /// Tries to extract the city without using database data.
    let cityRuleSimple (ri, m) =
        let f a =
            let b = tryAfterCity a
            b

        let retVal =
            match m.result, ri.rules with
            | Failed, _ -> failed (ri, m)
            | _ ->
                match m.unprocessed, m.address.zipCodeOpt, m.address.stateOpt, m.address.cityOpt with
                | [], _, _, _ -> ri, m // No input to work - ignore.
                | _, Some _, _, Some _ -> ri, m // Already have zip AND city.
                | _, _, Some _, Some _ -> ri, m // Already have state AND city.
                | _ :: _, Some _, Some _, None ->
                    let result = f (ri, m)
                    result
                | _ -> failed (ri, cityFailed m) // Must have zip OR state already resolved.

        retVal
