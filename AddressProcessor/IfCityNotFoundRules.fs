namespace Softellect.AddressProcessor

open MatchTypes
open StreetNameRules
open AddressTypes
open Extensions
open UnionFactories

/// The big question is, given, for example, an address like:
///     "123 MAIN ST WEST POINT CA 95255"
/// are the street name / city name: "MAIN ST" / "WEST POINT" or "MAIN ST WEST" / "POINT"?
///
/// Here are some real examples from EFStreetZips (note the projected words in the addresses below):
///     "123 1ST AVE W W MIFFLIN PA 15122" - the street name is "1ST AVE W"
///     "123 1ST AVE W HVN CT 06516" - the street name is "1ST AVE"
///     "123 1ST WAY W PALM BCH FL 33407" - the street name is "1ST WAY"
///
/// Here "W" is a part of the street name.
///     "123 1ST AVE W CTR AL 35960" - the street name is "1ST AVE W" !!!
///
/// All that can be matched if we have at least a city or street in the database.
module IfCityNotFoundRules =

    let ifCityNotFoundFailed m = { m with result = Failed; matchError = Some MatchError.InvalidAddress }
    let setIfCityNotFoundResult r m = { m with stepResults = { m.stepResults with cityResult = r } }


    /// The following rules must be satisfied:
    ///     1. Must have: (a) 1 or more words, e.g. "CHICOPEE" and (b) at least two more words for (street name + street type).
    ///     2. Must have no more than 4 words, e.g. "KINGS CANYON NATIONAL PK".
    ///     3. Must have a valid street type right BEFORE it, e.g. "ST", "AVE", etc...
    ///     4. Must have at least two words before (street name + street type).
    let tryGetCity u =
        let hasStreetType x =
            x
            |> streetTypeFactory.tryFromLabel
            |> Option.isSome

        let rec tryMatch acc rem =
            match acc with
            | _::_::_::_::_ -> None // Rule #2.
            | _ ->
                match rem with
                | [] -> None // Rule #1.b
                | h :: t ->
                    let s = hasStreetType h
                    match s, t with
                    | false, _ -> tryMatch (h :: acc) t
                    | true, _ -> Some (acc |> List.rev |> toCity, rem)

        match u with
        | [] | [ _ ] | _ :: [ _ ] | _ :: _ :: [ _ ] -> None // Rule #1.b.
        | a :: t -> tryMatch [ a ] t


    let ifCityNotFound m =
        match m.matchError with
        | None | Some CityNotFound | Some StreetNotFound ->
            match tryGetCity m.unprocessed with
            | Some (city, u) ->
                // If city does not exist, then AP may retry parsing without the zip code.
                // However, it will not find the city again, and currently it then leaves zip code in skipped.
                // Here we account for that.
                let address =
                    let zipCode =
                        match m.address.zipCodeOpt with
                        | Some z -> Some z
                        | None ->
                            m.currentSkipped
                            |> List.map ZipCode.tryCreate
                            |> List.tryPick id

                    { m.address with cityOpt = Some city; zipCodeOpt = zipCode }

                {
                    m with
                        result = Perfect
                        matchError = None
                        address = address
                        unprocessed = u
                        currentSkipped = []
                        stepResults = { m.stepResults with cityResult = Matched }
                }
            | None -> m
        | _ -> m


    let ifCityNotFoundRuleImpl f (ri, m) =
        let ri1, m1 = streetRule (ri, m)

        let retVal =
            let x =
                match m1.matchError with
                | Some CityNotFound | Some StreetNotFound | Some InvalidAddress ->
                    let m2 = ifCityNotFound m
                    ri, m2
                | _ -> ri1, ifCityNotFoundFailed m1

            f x

        retVal

    let ifCityNotFoundRule (ri, m) =
        let retVal =
            ifCityNotFoundRuleImpl id (ri, m)

        retVal
