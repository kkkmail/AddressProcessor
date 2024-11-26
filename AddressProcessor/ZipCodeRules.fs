namespace Softellect.AddressProcessor

open AddressTypes
open MatchTypes
open MatchingRules

module ZipCodeRules =

    let zipFailed m = { m with result = Failed; matchError = Some MatchError.ZipNotFound }
    let setZipResult r ri m = ri, { m with stepResults = { m.stepResults with zipResult = r } }


    let tryMatchZip (ri, m) =
        let withZipFailed () = failed (ri, zipFailed m)

        match m.unprocessed, m.resolved with
        | [], _ -> ri, m // No input to work - ignore.
        | h :: t, [] ->
            match ZipCode.tryCreate h with
            | Some z ->
                { m with unprocessed = t; address = { m.address with zipCodeOpt = Some z }; matchError = None }
                |> setZipResult Matched ri
            | None -> withZipFailed ()
        | h :: t, a :: _ ->
            match ZipCode.tryCreate h, a.resolvedAddress.zipCodeOpt with
            | Some z, _ ->
                { m with unprocessed = t; address = { m.address with zipCodeOpt = Some z }; matchError = None }
                |> setZipResult Matched ri
            | None, Some z ->
                { m with address = { m.address with zipCodeOpt = Some z }; matchError = None }
                |> setZipResult Inferred ri
            | None, None -> withZipFailed ()


    let zipRule (ri, m) =
        let retVal =
            match m.result with
            | Failed -> failed (ri, m)
            | _ ->
                match m.stepResults.numberResult, m.resolved with
                | Matched, a :: _ ->
                    // Previous numberRule matched. It is possible that we had something like "1960-80", which is now "1960" and it is castable to zip.
                    // Since numberRule matches, we override zip matching and require at least a valid zip 5 here.
                    // It is still possible to have some 5 digit street number, which will match zip and we will miss that.
                    match m.unprocessed,  a.resolvedAddress.zipCodeOpt with
                    | h :: _, Some z ->
                        // TODO :: We might want to plug in more "preview" rules here, similarly to numberMatch.
                        match h.Length with
                        | 0 | 1 | 2 | 3 | 4 ->
                            { m with address = { m.address with zipCodeOpt = Some z }; matchError = None }
                            |> setZipResult Inferred ri
                        | _ -> tryMatchZip (ri, m)
                    | _ -> tryMatchZip (ri, m) // Nothing really to do - let the main logic handle that.
                | _ -> tryMatchZip (ri, m)

        retVal


    /// Zip rule with no ZIP.
    /// If the first word could be zip, then it skips it.
    let zipRuleNoZip (ri, m) =
        let noZip g = { g with matchError = None } |> setZipResult NotMatched ri

        match m.unprocessed with
        | [] -> ri, m
        | h :: t ->
            match h |> ZipCode.couldBeZip with
            | true -> { m with currentSkipped = [ h ]; unprocessed = t }
            | false -> m
            |> noZip
