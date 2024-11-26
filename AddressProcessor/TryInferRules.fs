namespace Softellect.AddressProcessor

open System
open AddressTypes
open MatchTypes
open Extensions
open MatchingRules

module TryInferRules =

    /// A shortcut rule to speed up processing of address ranges or groups like "123, 125, 129 Main Street, ...".
    /// Otherwise it takes forever to process large ranges, e.g. "100-200 Main Street".
    let rec tryInferRule (ri, m) =
        match m.result with
        | Failed -> failed (ri, m)
        | _ ->
            match m.resolved with
            | [] -> ri, m
            | r :: _ ->
                let a = r.resolvedAddress
                // Must have at least one resolved address.
                match a.isValid, a.unitOpt, m.address.numberOpt, m.address.streetOpt, m.address.unitOpt, m.address.cityOpt, m.address.stateOpt with
                | true, None, None, None, None, None, None ->
                    // Must:
                    //     Have last resolved address valid AND it must NOT have a unit number.
                    //     NOT have yet resolved any of: house number / street / unit / city / state.
                    //         Zip code might be missing if it was found wrong.
                    match m.unprocessed with
                    | [] -> ri, m
                    | h ::t ->
                        match Int32.TryParse h, a.numberOpt |> Option.bind(fun e -> e.value |> Int32.TryParse |> Some) with
                        | (true, i), Some (true, j) when i < j ->
                            // Must have increasing integers as house numbers.
                            match Number.tryCreate [ h ] with
                            | Some n ->
                                // Reset all other rules as otherwise they usually pick up weird stuff after this rule.
                                let (r1, m1) = (ri.completed(), { m with address = { a with numberOpt = Some n }; unprocessed = t }) |> ri.newAddress
                                // Try to infer more house numbers.
                                tryInferRule (r1, m1)
                            | None -> ri, m
                        | _ -> ri, m
                | _ -> ri, m
