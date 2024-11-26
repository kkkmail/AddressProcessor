namespace Softellect.AddressProcessor

open AddressTypes
open MatchTypes
open Extensions
open MatchingRules

module UnitNumberRules =

    let unitFailed m = { m with result = Failed; matchError = Some InvalidUnitNumber }
    let setUnitResult r m = { m with stepResults = { m.stepResults with unitResult = r } }


    /// Validates that a unit number (n) matches list of strings (d.comparand) chosen from a list of strings (d.fullComparand).
    let unitNumberValidator k (d : BestMatchComparandData<list<string>>) (n : UnitNumber) =
        let defaultComparer() =
            match d.comparand.Length = k with
            | true ->
                match d.comparand |> List.rev |> UnitNumber.tryCreate = Some n with
                | true -> Perfect
                | false -> Failed
            | false -> Failed

        defaultComparer()


    let tryAfterUnitNumber (ri, m) =
        let applyRules (ri, m) =
            let rec inner (r, g) =
                match r.rules with
                | [] -> (r, g)
                | h :: t ->
                    let r1 = { r with rules = t }
                    let rm = h.apply (r1, g)
                    inner rm

            let a = inner (ri, m)
            a

        let processBoundRuleCollection f s rc =
            let a =
                rc
                |> List.choose (fun (r, e) -> e |> Option.bind (fun v -> Some (r, v)))
                |> List.mapi(fun i e -> i, applyRules e)
                |> List.map(fun (i, m) -> s i m, m)
                |> List.sortBy(fun (i, _)-> i)

            let b = a |> tryRuleCollectionHead f
            b

        match m.result with
        | Failed -> failed (ri, m)
        | _ ->
            let a = [ ri, Some m ]
            let b = a |> processBoundRuleCollection (fun () -> failed (ri, unitFailed m)) (defaultSorter ri.matchParams)
            b


    let tryMatchUnitNumber k m =
        match tryPickUnitNumber m.unprocessed (unitNumberValidator k) with
        | Some d, _ -> updateMatchInfo d m (fun a -> { a with unitOpt = Some d.value }) |> setUnitResult Matched
        | _ -> unitFailed m


    let unitRuleImpl t k (ri, m) =
        let withUnitFailed () = failed (ri, unitFailed m)

        let f a =
            let b = tryAfterUnitNumber a
            b

        let retVal =
            match m.result, ri.rules with
            | Failed, _ -> failed (ri, m)
            | _ ->
                match m.unprocessed, m.address.zipCodeOpt, m.address.stateOpt, m.address.cityOpt with
                | [], _, _, _ -> ri, m // No input to work - ignore.
                | _, _, _, None ->  withUnitFailed () // Must have city resolved.
                | _ ->
                    match m.stepResults.streetResult, m.stepResults.cityResult with
                    | Inferred, _ ->  withUnitFailed () // Can't have unit numbers if street was inferred.
                    | _, Inferred ->  withUnitFailed () // Can't have unit numbers if city was inferred.
                    | _ ->
                        let a = ri, t k m
                        f a

        retVal


    let unitTryMatchRule k (ri, m) =
        let retVal = unitRuleImpl tryMatchUnitNumber k (ri, m)
        retVal


    let unitRule (ri, m) =
        let c m = ri, unitFailed m

        let a =
            [
                unitTryMatchRule 1
                unitTryMatchRule 2
            ]
            |> processRuleCollection c (defaultSorter ri.matchParams) (ri, m)

        a


    /// Unit Number rule, which does nothing.
    let unitIdRule (ri, m) = ri, m
