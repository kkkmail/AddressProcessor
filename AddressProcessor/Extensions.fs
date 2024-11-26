namespace Softellect.AddressProcessor

open System
open UnionFactories
open DatabaseTypes
open AddressTypes
open Dictionaries
open StringParser

module Map =

    /// https://stackoverflow.com/questions/50804004/what-is-the-behaviour-of-map-ofseq-with-duplicate-keys/50804072
    let ofListWithDuplicates resolver lst =
        let rec getDuplicates state remaining =
            match remaining with
            | (key, value) :: tail ->
                let newState =
                    match state |> Map.tryFind key with
                    | Some existing -> state |> Map.add key (value :: existing)
                    | None -> state |> Map.add key [ value ]
                getDuplicates newState tail
            | [] -> state

        lst
        |> getDuplicates Map.empty
        |> Map.map (fun key values -> values |> resolver key)


module Extensions =

    type State
    with
        static member tryCreate (s : string) : State option = stateFactory.tryFromKey s

        static member tryCreate (z : ZipCode, s : string) : State option =
            match State.tryCreate s with
            | Some t ->
                match z with
                | ZipCode z ->
                    match allZipStates.TryFind z with
                    | Some st ->
                        match st.Contains t with
                        | true -> Some t
                        | false -> None
                    | None -> None
            | None -> None

        member this.key = stateFactory.getKey this


    type StreetType
    with
        member this.key = streetTypeFactory.getKey this
        static member tryCreate (s : string) = streetTypeFactory.tryFromKey s

        // All street types, which can be swapped with the street name, like "HWY 41".
        static member allCanBeMid =
            StreetAbbr.all
            |> Array.filter (fun e -> e.canBeMid)
            |> Array.map (fun e -> e.caseValue, e.caseValue)
            |> Array.distinct
            |> Map.ofArray


    type Direction
    with
        member this.key = directionFactory.getKey this
        static member tryCreate (s : string) = directionFactory.tryFromKey s
        member this.value = this.key // For standardization


    /// Should not really be here but due to dependecy on allStreetAbbrMap and allDirectionMap it is diffucult to place.
    /// TODO kk:20191107 - Perhaps it is worth doing that dynamically when looking for a unit number in a unit number rule. Refactor if needed.
    /// So, at this point we remove "suite name" from the list of words if it is followed by a valid "suite number".
    /// We don't care if it is called SUITE, LOT, #, UNIT, etc...
    /// For example:
    ///     When we have "UNIT ST", then we don't want to remove "suite name" "UNIT" from the list.
    ///     But when we have "UNIT 1F", then we want to remove the word "UNIT".
    let standardizeList (s : list<string>) =
        let tryRemoveSuiteName i =
            let rec inner acc e =
                let tryNext h t = inner ((Some h)::acc) t

                match e with
                | [] -> acc
                | h ::t ->
                    match allSuiteNames.Contains h with
                    | true ->
                        match t with
                        | [] -> tryNext h t
                        | h1::t1 ->
                            match isUnitNumber h1 with
                            | true -> tryNext h1 t1 // Remove suite "name" since it is followed by a valid suite "number".
                            | false -> tryNext h t
                    | false -> tryNext h t

            inner [] i
            |> List.choose id
            |> List.rev

        let x =
            s
            |> List.map (fun e -> match allStreetAbbrMap.TryFind e with | Some a -> a.key | None -> e)
            |> List.map (fun e -> match allDirectionMap.TryFind e with | Some a -> a.key | None -> e)
            |> List.map (fun e -> match originalAbbr.TryFind e with | Some a -> a | None -> e)
            |> List.map (fun e -> match standardAbbr.TryFind e with | Some a -> a | None -> e)
            |> tryRemoveSuiteName

        x

    /// TODO kk:20190529 - Implement
    /// Un-projects standard abbreviated representations of SOME words into full words.
    let unProject (i : UnProjectInfo) (s : list<string>) =
        s


    /// ! Note that it effectively applies List.rev !
    let streetFolder s = (s |> List.fold (fun acc r -> r + DefaultSplitCharacter + acc) EmptyString).Trim()


    let cityFolder = streetFolder


    /// ! Note that it effectively applies List.rev !
    let toStreet s =
        let street = s |> streetFolder
        let streetOriginal = s |> (unProject UnProjectInfo.defaultValue) |> streetFolder
        Street { projectedName = street; originalName = Some streetOriginal }


    let toCity s =
        let city = s |> cityFolder
        let cityOriginal = s |> (unProject UnProjectInfo.defaultValue) |> cityFolder
        City { projectedName = city; originalName = Some cityOriginal }


    /// Need to remove extra words, which are irrelevant for street numbers, for example: "AND"
    let standardizeListForStreetNumber sl =
        standardizeList sl


    let standardize i (s : string) =
        s.ToUpper()
        |> cleanString i
        |> defaultSplit
        |> standardizeList


    /// Standardizes string and returns PROJECTED representation.
    let standardizeString i s =
        s
        |> standardize i
        |> combineIfNotTooLong i.maxNumberOfWords


    /// Standarizes ORIGINAL string.
    /// Only a very small number of transformations is allowed.
    let standardizeOriginalString _ so =
        match so with
        | None -> None
        | Some s ->
            s
            |> defaultSplit
            |> List.map (fun e -> match originalAbbr.TryFind e with | Some a -> a | None -> e)
            |> combineIfNotTooLong None
            |> Some


    /// Checks that the name contains at least one street abbreviation word.
    let streetNameValidator s =
        s
        |> defaultSplit
        |> List.fold (fun acc r -> acc || allStreetAbbrMap.ContainsKey r || allStandardGroups.Contains r) false


    /// Extra cleaner for unit numbers.
    let standardizeUnitNumberString s =
        s
        |> standardize CleanStringParams.unitNumberValue
        |> combineIfNotTooLong None


    /// TODO kk:20191122 - Implement.
    /// Removes unit number from the end of the street name if it is there:
    /// For example:
    ///     "WAREHAM ST PH2" -> "WAREHAM ST"
    ///     "7 AVE 10EF" -> "7 AVE"
    let cleanUnitNumberFromStreetName (s : string) : string =
        s


    let applyCreator i s c =
        match standardizeString i s with
        | EmptyString -> None
        | n -> c n |> Some


    let applyCreatorWithOriginal i (s, o) c =
        match standardizeString i s with
        | EmptyString -> None
        | n -> c { projectedName = n; originalName = standardizeOriginalString i o } |> Some


    let applyCreatorWithOriginalAndValidation v i (s, o) c =
        match standardizeString i s with
        | EmptyString -> None
        | n ->
            match v n with
            | true -> applyCreatorWithOriginal i (s, o) c
            | false -> None


    let toStreetCleanStringParam i =
        match i.maxNumberOfWords with
        | Some _ -> i
        | None -> { i with maxNumberOfWords = Some 7 }


    type Street
    with
        static member tryCreate i (s, o) = applyCreatorWithOriginal (toStreetCleanStringParam i) (s, o) Street
        static member tryCreateFromStreetZipDetailed i (r : StreetZipDetailed) = Street.tryCreate i (r.StreetFullName, r.StreetOriginalName)

        static member tryCreateCleaned i (s, o) =
            Street.tryCreate i (cleanUnitNumberFromStreetName s, o |> Option.bind (fun e -> e |> cleanUnitNumberFromStreetName |> Some))

        static member tryCreateCleanedFromStreetZipDetailed i (r : StreetZipDetailed) =
            Street.tryCreateCleaned i (r.StreetFullName, r.StreetOriginalName)

        static member tryCreateWithValidation i (s, o) = applyCreatorWithOriginalAndValidation streetNameValidator (toStreetCleanStringParam i) (s, o) Street

        static member tryCreateFromStreetZipDetailedWithValidation i (r : StreetZipDetailed) =
            Street.tryCreateWithValidation i (r.StreetFullName, r.StreetOriginalName)


    type City
    with
        static member tryCreate i (s, o) = applyCreatorWithOriginal i (s, o) City
        static member tryCreateFromStreetZipDetailed i (r : StreetZipDetailed) = City.tryCreate i (r.City, r.CityOriginalName)


    type Number
    with
        static member tryCreateImpl creators g =
            let s = String.concat DefaultSplitCharacter g // Glue [ "1234"; "1"; "/"; "2" ] into "1234 1 / 2"

            match s with
            | EmptyString -> None
            | n ->
                creators
                |> List.tryFind (fun (e, _) -> e n)
                |> Option.bind (fun (_, e) -> e n |> Number |> Some)


        static member tryCreate g =
            let creators =
                [
                    isRegularHouseNumber, id
                    isGridHouseNumber, fun (x : string) -> x.Replace(" ", "") // Merge "N 100 W 1000" into "N100W1000"
                    isHalfIntegerHouseNumber, fixHalfNumbers
                ]

            Number.tryCreateImpl creators g


        static member tryCreateRelaxed g =
            let creators =
                [
                    isRegularRelaxedHouseNumber, id
                    isGridHouseNumber, fun (x : string) -> x.Replace(" ", "") // Merge "N 100 W 1000" into "N100W1000"
                    isHalfIntegerHouseNumber, fixHalfNumbers
                ]

            Number.tryCreateImpl creators g


    type UnitNumber
    with
        static member tryCreate g =
            let s = String.concat DefaultSplitCharacter g // Glues [ "F"; "1" ] into "F 1"

            // TODO kk:20191122 - standardizeUnitNumberString expands e.g. "F1" into "F 1", which we then eventually collapse back. Refactor.
            match (standardizeUnitNumberString s) with
            | EmptyString -> None
            | n ->
                match allStreetAbbrMap.ContainsKey s with
                | true -> None // e.g. "DR" is not a unit number but it might be picked up as potential unit number when skipped due to some reasons.
                | false ->
                    match isUnitNumber n with
                    | true ->
                        // kk:20191125 - At this point it is currently safe to replace space in "F 1" by empty string
                        // since strings like "1 1" are not valid unit numbers. Refactor if this changes.
                        let m = n.Replace(DefaultSplitCharacter, EmptyString)
                        UnitNumber m |> Some
                    | false -> None


    type County
    with
        static member tryCreate i (fips : int, name : string, state : string) =
            match stateFactory.tryFromKey state with
            | Some s ->
                {
                    fipsCode = fips
                    countyName = name |> standardizeString i
                    state = s
                }
                |> Some
            | None -> None


    type FullAddressRaw
    with
        static member tryCreate i (s, o) = applyCreatorWithOriginal i (s, o) FullAddressRaw


    type StreetNameRaw
    with
        static member tryCreate i (s, o) = applyCreatorWithOriginal i (s, o) StreetNameRaw

        /// Creates street, which is guaranteed to contain at least one street abbreviation word.
        static member tryCreateWithValidation i (s, o) =
            applyCreatorWithOriginalAndValidation streetNameValidator i (s, o) StreetNameRaw


    type Array
    with
        static member tryBeforeLast (a : array<'T>) =
            match a.Length with
            | 0 | 1 -> None
            | _ -> Some (a.[a.Length - 2])


    type TimeSpan
    with
        member this.DivideBy (d : int64) : TimeSpan =
            TimeSpan.FromTicks(this.Ticks / d)

        member this.MultiplyBy (d : int64) : TimeSpan =
            TimeSpan.FromTicks(this.Ticks * d)
