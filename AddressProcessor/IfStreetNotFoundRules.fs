namespace Softellect.AddressProcessor

open MatchTypes
open StreetNameRules
open AddressTypes
open Extensions
open UnionFactories

/// The main problem is how to find where to stop, e.g. (note the glued "10" and "W"):
///    "..., 6 8 10W 12TH ST, New York, NY, 10001"
module IfStreetNotFoundRules =

    let ifStreetNotFoundFailed m = { m with result = Failed; matchError = Some MatchError.InvalidAddress }
    let setIfStreetNotFoundResult r m = { m with stepResults = { m.stepResults with streetResult = r } }


    /// The following rules must be satisfied:
    ///     1. Must have 2 or more words, e.g. "MAIN ST".
    ///     2. Must have no more than 7 words, e.g. "DR MARTHIN LUTHER KING JR DR S".
    ///     3. Must have a valid street type, e.g. "ST", "AVE", etc...
    ///     4. Must have (a) a valid INTEGER house number (b) BEFORE street name. Half integer and grid numbers are currently not supported.
    ///     5. If there is something else left, then it must be either a valid INTEGER house number (a) OR a valid ZIP (b).
    ///
    /// Note that the street type can be fairly far, e.g.: "US HWY 50 S".
    let tryGetStreet u =
        let hasStreetType x =
            x
            |> List.tryPick streetTypeFactory.tryFromLabel
            |> Option.isSome

        let rec tryMatch acc rem =
            let matched() = Some (acc |> List.rev |> toStreet, rem)
            let notMatched a b = tryMatch a b

            match acc with
            | _::_::_::_::_::_::_::_::_ -> None // Rule #2.
            | _ ->
                match rem with
                | [] -> None // Rule #4.a
                | h :: t ->
                    let n = Number.tryCreate [ h ]
                    let s = hasStreetType acc
                    match n, s with
                    | Some _, true ->
                        // Rules ##3, 4.b
                        match t with
                        | [] -> matched()
                        | h1 :: _ ->
                            match Number.tryCreate [ h1 ], ZipCode.tryCreate h1 with
                            | Some _, _ -> matched() // Rule #5.a
                            | _, Some _ -> matched() // Rule #5.b
                            | _ -> notMatched (h :: acc) t
                    | _ -> notMatched (h :: acc) t

        match u with
        | [] | [ _ ] | _ :: [ _ ] -> None // Rule #1.
        | a :: b :: t ->
            // Note that t here is not [] and a :: b must be a part of the street name.
            tryMatch [ b; a ] t


    let ifStreetNotFound m =
        match m.matchError with
        | Some StreetNotFound ->
            match tryGetStreet m.unprocessed with
            | Some (street, u) ->
                // If street does not exist, then AP may retry parsing without the zip code.
                // However, it will not find the street again, and currently it then leaves zip code in skipped.
                // Here we account for that.
                let address =
                    let zipCode =
                        match m.address.zipCodeOpt with
                        | Some z -> Some z
                        | None ->
                            m.currentSkipped
                            |> List.map ZipCode.tryCreate
                            |> List.tryPick id

                    { m.address with streetOpt = Some street; zipCodeOpt = zipCode }

                {
                    m with
                        result = Perfect
                        matchError = None
                        address = address
                        unprocessed = u
                        currentSkipped = []
                        stepResults = { m.stepResults with streetResult = NotMatched }
                }
            | None -> m
        | _ -> m


    let ifStreetNotFoundRuleImpl f (ri, m) =
        let ri1, m1 = streetRule (ri, m)

        let retVal =
            let x =
                match m1.matchError with
                | Some StreetNotFound ->
                    let m2 = ifStreetNotFound m1
                    ri, m2
                | _ -> ri1, ifStreetNotFoundFailed m1

            f x

        retVal

    let ifStreetNotFoundRule (ri, m) =
        let retVal =
            ifStreetNotFoundRuleImpl id (ri, m)

        retVal


    let ifStreetNotFoundRuleWithTrySetCity (ri, m) =
        let retVal =
            ifStreetNotFoundRuleImpl trySetCity (ri, m)

        retVal
