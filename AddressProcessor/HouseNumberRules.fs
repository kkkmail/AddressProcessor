namespace Softellect.AddressProcessor

open AddressTypes
open MatchTypes
open Extensions
open MatchingRules
open StreetNameRules

module HouseNumberRules =

    let numberFailed m = { m with result = Failed; matchError = Some CityNotFound }
    let setNumberResult r m = { m with stepResults = { m.stepResults with numberResult = r } }


    let defaultComparer d n =
        match d.comparand |> List.rev |> Number.tryCreate = Some n with
        | true -> Perfect
        | false -> Failed


    /// Here we currently expect only OnlySkippedTail case. However...
    let isExactMatch (d : BestMatchComparandData<list<string>>) n =
        match d.fullComparandType with
        | NoSkippedTail ->
            // Can't realy do anything since we don't have a tail to work with.
            false
        | OnlySkippedTail ->
            // Must have no tail and defaultComparer must have exact match.
            d.fullComparand.Length = 0 && defaultComparer d n = Perfect
        | IncludeSkippedTail ->
            // We should not be here. Return false for the time being. This can be tweaked to account for both.
            false


    /// Validates that a house number (n) matches list of strings (d.comparand) chosen from a list of strings (d.fullComparand).
    let houseNumberValidator (i : HouseNumberValidatorInputData) (d : BestMatchComparandData<list<string>>) (n : Number) =
        let defaultComparer() = defaultComparer d n
        let isExactMatch() = isExactMatch d n
        let validateStreet = validateStreet i.fuzzySearchParams.streetSearchParams.streetSearchType

        match i.addressData with
        | None -> defaultComparer()
        | Some a ->
            match isExactMatch() with
            | true -> defaultComparer()
            | false ->
                // If we are here then d.fullComparand contains some extra words e.g. [ "MAIN"; "1" ] or [ "GARAGE"; "1" ] (note the reversed order).
                // The "1" must be matched by a house number.
                match tryPickMatch i.wordMap i.streetCityStates d.fullComparand (validateStreet (i.toStreetValidationInputData a.cityOpt)) with
                | Some _, r ->
                    match r with
                    | Perfect -> Failed // We can create a street name out of skipped words.
                    | Partial p ->
                        match i.previous.streetResult with
                        | Inferred ->
                            // We already created full address, so we must be careful with lines like "123 MAIN 456 ST GEORGE"
                            // So, after "456 ST GEORGE" we can't really skip "MAIN".
                            match p <= HouseNumberInferredThreshold with
                            | true -> Failed // We can create a street name with good enough score out of skipped words.
                            | false -> defaultComparer() // The match score is low.
                        | _ -> defaultComparer() // This is a "first" match. Allow to skip more...

                    | Failed -> defaultComparer()
                | None, _ -> defaultComparer()


    let numberRuleImpl t (ri, m) =
        let retVal =
            match m.result, ri.rules with
            | Failed, _ -> failed (ri, m)
            | _ ->
                match m.unprocessed, m.address.zipCodeOpt, m.address.stateOpt, m.address.cityOpt, m.address.streetOpt with
                | [], _, _, _, _ -> ri, m // No input to work - ignore.
                | _, None, None, None, _  | _, None, None, Some _, _ | _, None, Some _, None, _ -> ri, numberFailed m // Must have zip OR city + state already resolved.
                | _, _, _, _, None -> failed (ri, numberFailed m) // Must have street already resolved.
                | _ :: _, _, _, _, _ -> ri, t (ri, m)

        retVal


    let tryMatchNumber (ri, m) =
        let st, w = getMaps ri m.address
        let i = { streetCityStates = st; addressData = Some m.address; wordMap = w; previous = m.stepResults; fuzzySearchParams = ri.matchParams.fuzzySearchParams }

        match tryPickHouseNumber m.unprocessed (houseNumberValidator i) with
        | Some d, _ -> updateMatchInfo d m (fun a -> { a with numberOpt = Some d.value }) |> setNumberResult Matched
        | None, _ -> numberFailed m


    let numberTryMatchRule (ri, m) =
        let retVal = numberRuleImpl tryMatchNumber (ri, m)
        retVal


    let numberRule (ri, m) =
        let c m = ri, numberFailed m

        let a =
            [
                numberTryMatchRule
            ]
            |> processRuleCollection c (defaultSorter ri.matchParams) (ri, m)

        a
