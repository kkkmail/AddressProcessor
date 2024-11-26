namespace Softellect.AddressProcessor

open AddressTypes
open MatchTypes
open Extensions
open MatchingRules

module StateRules =

    let stateFailed m = { m with result = Failed; matchError = Some MatchError.StateNotFound }
    let setStateResult r ri m = ri, { m with stepResults = { m.stepResults with stateResult = r } }


    /// Checks that last word is a valid state.
    let stateRule (ri, m) =
        let withStateFailed () = failed (ri, stateFailed m)

        let retVal =
            match m.result with
            | Failed -> failed (ri, m)
            | _ ->
                match m.unprocessed, m.address.zipCodeOpt with
                | [], _ -> ri, m // No input to work - ignore.
                | h :: t, Some z ->
                    // If zip was matched or inferred then use that zip to validate state.
                    match State.tryCreate (z, h) with
                    | Some st ->
                        { m with unprocessed = t; address = { m.address with stateOpt = Some st }; matchError = None }
                        |> setStateResult Matched ri
                    | None ->
                        match m.stepResults.zipResult, m.resolved with
                        | Inferred, hr :: _ ->
                            // ZIP was inferred ==> infer state as well.
                            { m with address = { m.address with stateOpt = hr.resolvedAddress.stateOpt }; matchError = None }
                            |> setStateResult Inferred ri
                        | Matched, _ ->
                            // ZIP was matched ==> ignore
                            { m with matchError = None }
                            |> setStateResult NotMatched ri
                        | _ ->
                            // ZIP did not match / was not inferred and now state did not match as well.
                            withStateFailed ()
                | h :: t, None ->
                    // No ZIP from previous step.
                    match State.tryCreate h with
                    | Some st ->
                        { m with unprocessed = t; address = { m.address with stateOpt = Some st }; matchError = None }
                        |> setStateResult Matched ri
                    | None ->
                        match m.resolved with
                        | hr :: _ ->
                            // Infer state.
                            { m with address = { m.address with stateOpt = hr.resolvedAddress.stateOpt }; matchError = None }
                            |> setStateResult Inferred ri
                        | [] -> withStateFailed () // State did not match and cannot be inferred.

        retVal
