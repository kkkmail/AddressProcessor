namespace Softellect.AddressProcessor
open System

open Dictionaries
open UnionFactories
open StringParser
open Swyfft.Common.SetDefinitions.CommonSets

module AddressTypes =

    [<Literal>]
    let DefaultSuiteName = "#"


    /// Returns the norm of the list of weighted words.
    /// If all words are "standard", then the norm is equal to the count of words.
    let norm (len : int) (st : List<string * float>) =
        if len > 0 then (st |> List.fold (fun acc (_, i) -> acc + i) 0.0)
        else 0.0 // Don't want an empty set.


    let streetNameOrTypeWeight = 0.8
    let minStreetAbbrWeight = 0.55
    let maxStreetAbbrWeight = 0.75


    /// Weights of street abbreviations besed on the frequency of occurrence.
    let streetAbbrWeight =
        let takeLog v = if v > 0 then v |> float |> log else 0.0

        let x =
            StreetAbbr.streetTypeOccurrence
            |> List.filter (fun (e, n) -> (allStreetAbbrMap.ContainsKey e) && (n > 0))

        let minVal = x |> List.map (fun (_, e) -> e) |> List.min |> takeLog
        let maxVal = max 1.0 (x |> List.map (fun (_, e) -> e) |> List.max |> takeLog)
        let interval = maxVal - minVal

        let getWeight w =
            let w1 = max 0.0 (min 1.0 (maxVal - (takeLog w)) / interval)
            minStreetAbbrWeight + (maxStreetAbbrWeight - minStreetAbbrWeight) * w1

        x
        |>
        List.map (fun (a, b) -> a, getWeight b)
        |> Map.ofList


    /// Type to control if we want to use projected, original, or both.
    type ProjectionUsage =
        | ProjectedOnly
        | ProjectedAndOriginal
        | OriginalOnly


    /// kk:20200114 - The generalization to remove more than one space seems to introduce a performance hit without extreme need. Revisit if that becomes needed.
    type ToSublistsType =
        | WordsOnly // Use only the input list of words
        | WordsAndRemovedSpace // Use the input list of words AND try to remove one single space, e.g. ["PEAK"; "MEADOW"] -> ["PEAKMEADOW"]


    /// Controls how to convert the list of words into sublists.
    type ToSublistsParams =
        {
            projectionUsage : ProjectionUsage
            toSublistsType : ToSublistsType
        }

        static member exactMatchValue =
            {
                projectionUsage = ProjectedOnly
                toSublistsType = WordsAndRemovedSpace
            }

        static member fuzzyMatchValue =
            {
                projectionUsage = ProjectedAndOriginal
                toSublistsType = WordsAndRemovedSpace
            }

        static member wordsOnlyValue =
            {
                projectionUsage = ProjectedAndOriginal
                toSublistsType = WordsOnly
            }


    /// Calculates the weight of the word by checking various dictionaries.
    let getWeight s =
        match allStreetAbbrMap.ContainsKey s, allDirectionMap.ContainsKey s with
        | false, false -> 1.0
        | true, false ->
            match StreetAbbr.streetNameOrTypeSet.Contains s with
            | true -> streetNameOrTypeWeight
            | false ->
                match streetAbbrWeight.TryFind s with
                | Some v -> v
                | None -> maxStreetAbbrWeight
        | false, true ->
            match StreetAbbr.streetNameOrTypeSet.Contains s with
            | true ->     0.90 // Does not seem to exist.
            | false ->    0.85 // We want to be able to match "SOUTH DR" just by "SOUTH" but not just by "DR".
        | true, true ->   0.50 // That does not exist.


    let demote l = l |> List.choose id
    let demote2 l = l |> List.map (fun (a, b) -> a |> Option.bind (fun c -> Some (c, b))) |> List.choose id
    let elevate l = l |> List.map Some
    let elevate2 l = l |> List.map (fun (a, b) -> Some a, b)
    let isValid l = l |> demote |> elevate = l
    let isValid2 l = l |> demote2 |> elevate2 = l


    type ListConstraint =
        {
            listLength : int
            subListConstraints : List<(int * float)>
        }


    /// Minimum allowed weights based on list and sublist lengths.
    /// The weight MUST BE multiplied by a norm of the list to get allowed norm.
    /// How should we match "DR MARTIN LUTHER KING JR DR WEST" or something like that?
    let allSubListConstraints =
        [
            {
                listLength = 1

                subListConstraints =
                    [
                        (1, 0.4999) // There should be no single word street name + street abbreviation.
                    ]
            }

            {
                listLength = 2

                subListConstraints =
                    [
                        (2, 0.4999)
                        (1, 0.5001) // Can skip "ST" in "MAIN ST" but not "MAIN".
                    ]
            }

            {
                listLength = 3

                subListConstraints =
                    [
                        (3, 0.4999)
                        //(2, 0.6100) // 0.6666 requires all words to be equal, 0.6144 is the value of "BEACH AVE" in "SOUTH BEACH AVE", 0.6243 is the value of "NW HWY" in "N NW HWY".
                        (2, 0.5294) // 0.6666 requires all words to be equal, 0.6144 is the value of "BEACH AVE" in "SOUTH BEACH AVE", 0.6243 is the value of "NW HWY" in "N NW HWY".
                        (1, 1.0000) // We might want to adjust it down to allow matching "ST GEORGE ST" just by "GEORGE", which has value of 0.4762.
                    ]
            }

            {
                listLength = 4

                subListConstraints =
                    [
                        (4, 0.4999)
                        (3, 0.7499) // 0.7499 allows all words to be equal == can skip any one word.
                        (2, 1.0000) // Don't want to match by two words.
                        (1, 1.0000) // Don't want to match by a single word.
                    ]
            }

            {
                listLength = 5

                subListConstraints =
                    [
                        (5, 0.4999)
                        (4, 0.7999) // 0.7999 allows all words to be equal == can skip any one word.
                        (3, 0.6500) // Can skip two less important words, like "ST" and "SW".
                        (2, 1.0000) // Don't want to match by two words.
                        (1, 1.0000) // Don't want to match by a single word.
                    ]
            }
        ]
        |> List.map (fun e -> e.listLength, e.subListConstraints |> Map.ofList)
        |> Map.ofList


    let getSubListWeight allTotal allLength subLength =
        let defaultWeight = 0.7499

        match allSubListConstraints.TryFind allLength with
        | Some v ->
            match v.TryFind subLength with
            | Some f -> allTotal * f
            | None -> allTotal * defaultWeight
        | None -> allTotal * defaultWeight

    let private getValidSubListsImpl getAllSubAddresses (all : list<string * float>) =
        let total = norm all.Length all
        let allSubAddresses = all |> getAllSubAddresses
        let filteredLists = allSubAddresses |> Set.filter (fun e ->
            let subTotal = norm all.Length e
            let threshold = getSubListWeight total all.Length e.Length
            subTotal > threshold)

        let subAddresses = filteredLists |> Set.map (fun e -> e |> List.map (fun (addr, weight) -> addr))
        subAddresses


    let private toValidSubLists splitter (partsLst : list<string>) =
        let totalWeight =
            match partsLst.Length with
            | 0 -> 1.0
            | _ -> (partsLst |> List.fold (fun acc r -> acc + (getWeight r)) 0.0)

        partsLst
        |> List.map (fun e -> (e, (getWeight e) / totalWeight))
        |> getValidSubListsImpl splitter


    /// Tries to merge two adjacent words and returns all possible combinations.
    /// If the list has 0 or 1 words, then returns the original list.
    let tryRemoveSingleSpace (parts : list<string>) =
        match parts.Length with
        | 0 | 1 -> [ parts ]
        | n ->
            let merge a (b : list<string>) =
                match b with
                | [] -> None
                | _ :: [] -> None
                | h :: h1 :: t -> Some (a @ ((h + h1) :: t))

            [ for i in 0..(n - 2) -> List.splitAt i parts ]
            |> List.map (fun (a, b) -> merge a b)
            |> List.choose id


    /// When we have multiple words, which can be street abbreviations e.g. "BEACH AVE",
    /// then without this normalization single word matches by "BEACH" were disallowed.
    /// Note that the weights above allow matching "SOUTH BEACH" by "SOUTH" but not by "BEACH".
    /// Revisit if needed.
    let private toValidSubListsImpl (t : ToSublistsParams) splitter (parts : list<string>) =
        let toValidSubLists = toValidSubLists splitter

        match t.toSublistsType with
        | WordsOnly -> toValidSubLists parts
        | WordsAndRemovedSpace ->
            parts :: (parts |> tryRemoveSingleSpace)
            |> List.map toValidSubLists
            |> List.fold (fun acc e -> Set.union acc e) Set.empty


    let toValidSortedSubLists p = toValidSubListsImpl p setOfSortedSublists
    let toValidUnsortedSubLists p = toValidSubListsImpl p setOfUnsortedSublists


    /// Do not merge the pieces as sometimes we need to examine them during debugging.
    let matchEnds x y =
        let a = x |> List.tryHead
        let a1 = y |> List.tryHead
        let b = x |> List.rev |> List.tryHead
        let b1 = y |> List.rev |> List.tryHead

        match a, a1, b, b1 with
        | Some av, Some av1, Some bv, Some bv1 ->
            let retVal = ((av = av1) && (bv = bv1))
            retVal
        | _ -> false


    /// Create valid sublists, which have the same ends as the original list.
    /// Order of words is preserved.
    let toValidSubListsWithMatchingEnds t p =
        let a = toValidUnsortedSubLists t p
        let b = a |> Set.filter(fun e -> matchEnds p e)
        b


    let inline asString<'T when 'T : (member value : string)> (tOpt : 'T option ) =
        match tOpt with
        | Some t -> (^T : (member value : string) (t))
        | None -> EmptyString


    let inline asOriginalString<'T when 'T : (member originalValue : string)> (tOpt : 'T option ) =
        match tOpt with
        | Some t -> (^T : (member originalValue : string) (t))
        | None -> EmptyString


    type AddressKey =
        | AddressKey of string

        member this.value = let (AddressKey v) = this in v

        static member tryCreate (s : string) : AddressKey option =
            match s.Length with
            | 11 -> AddressKey s |> Some // All of them have length = 11.
            | _  -> None


    type ZipCode =
        | ZipCode of string

        member this.value = let (ZipCode v) = this in v

        // Can handle:
        //     Zip 5, like "12345"
        //     Zip 5 w/o leading zero, like "1234" instead of "01234"
        //     Zip 9, like "123456789"
        //     Zip-9, like "12345-6789"
        static member tryCreate (s : string) : ZipCode option =
            let tryToZip z =
                match allZipStates.TryFind z with
                | Some _ -> ZipCode z |> Some
                | None -> None

            let tryFromZip9 z (a : string) =
                match a |> Int32.TryParse |> fst with
                | true -> z |> tryToZip
                | false -> None

            match s.Length with
            | 0 -> None // In case we get an empty string
            | n when n < 5 -> s.PadLeft(5, '0') |> tryToZip
            | 5 -> s |> tryToZip
            | 9 -> tryFromZip9 (s.Substring(0, 5)) (s.Substring(5))
            | 10 ->
                match s.Substring(5, 1) with
                | "-" -> tryFromZip9 (s.Substring(0, 5)) (s.Substring(6))
                | _ -> None
            | _ -> None

        // Tests if string could be zip without validating that such zip actually exists.
        // This is needed for invalid zips.
        static member couldBeZip (s : string) : bool =
            match s.Length with
            | 0 -> false
            | 1 | 2 | 3 | 4 | 5 | 9 -> s |> Int32.TryParse |> fst
            | 10 ->
                match s.Substring(5, 1) with
                | "-" -> (s.Substring(0, 5) |> Int32.TryParse |> fst) && (s.Substring(6) |> Int32.TryParse |> fst)
                | _ -> false
            | _ -> false

        static member tryCreateNotValidated (s : string) : ZipCode option =
            match ZipCode.couldBeZip s with
            | true -> s.Substring(0, min 5 s.Length).PadLeft(5, '0') |> ZipCode |> Some
            | false -> None

        /// Exclusively for C#.
        static member cSharpCreate (s : string) : ZipCode =
            match ZipCode.tryCreateNotValidated s with
            | Some z -> z
            | None -> failwith $"Cannot create zip code from '%s{s}'."


    /// TODO kk:20191122 - Once HouseNumber below is implemented this function will become obsolete.
    let fixHalfNumbers (s : string) = s.Replace(" 1 / 2", " 1/2")


    //type HouseNumber =
    //    | IntegerNumber of int // e.g. "1931"
    //    | HalfIntegerNumber of int // e.g. "1931 1/2""
    //    | GridNumber of int * int // e.g. "N20W22951"


    type Number =
        | Number of string

        member this.value = let (Number v) = this in v |> fixHalfNumbers


    type AddressPartWithOriginal =
        {
            projectedName : string
            originalName : string option
        }


    type Street =
        | Street of AddressPartWithOriginal

        member this.value = let (Street v) = this in v.projectedName
        member this.originalValue = let (Street v) = this in getValueOrDefault v.projectedName v.originalName
        member this.toList = this.value |> defaultSplit
        member this.toOriginalList = this.originalValue |> defaultSplit
        member this.toValidSortedSubLists t = this.toList |> toValidSortedSubLists t
        member this.toValidOriginalSortedSubLists t = this.originalValue |> defaultSplit |> toValidSortedSubLists t


    type UnitNumber =
        | UnitNumber of string

        member this.value = let (UnitNumber v) = this in DefaultSuiteName + v
        member this.rawValue = let (UnitNumber v) = this in v


    type City =
        | City of AddressPartWithOriginal

        member this.value = let (City v) = this in v.projectedName
        member this.originalValue = let (City v) = this in getValueOrDefault v.projectedName v.originalName
        member this.toList = this.value |> defaultSplit
        member this.toOriginalList = this.originalValue |> defaultSplit
        member this.toValidSortedSubLists t = this.toList |> toValidSortedSubLists t
        member this.toValidOriginalSortedSubLists t = this.originalValue |> defaultSplit |> toValidSortedSubLists t


    type County =
        {
            fipsCode : int
            countyName : string
            state : State
        }


    type Location =
        {
            latitude : decimal
            longitude : decimal
        }

        static member tryCreate (lat : string, lon : string) : Location option =
            match lat |> Decimal.TryParse, lon |> Decimal.TryParse with
            | (true, latitude), (true, longitude) -> Some { latitude = latitude; longitude = longitude }
            | _ -> None

        static member tryCreate (lat : decimal, lon : decimal) : Location option =
            match lat, lon with
            | 0m, _ -> None
            | _, 0m -> None
            | _ -> Some { latitude = lat; longitude = lon }


    type StreetCityState =
        {
            street : Street
            city : City
            state : State
        }


    /// Type to describe if the address was in the original full addres string
    /// or it was added (= infered) when ranges where expanded.
    type AddressInferenceType =
        | ExplicitAddress // The address was present in the original address string and was found with minimal guessing, e.g. "202-208" -> [ "202"; "208" ]
        | ImplicitEvenOrOddAddress // The address comes from the middle of even-even or odd-odd ranges, e.g. "202-208" -> [ "204"; "206" ]
        | ImplicitAllAddress // The address comes from the middle of even-odd or odd-even ranges, e.g. "202-207" -> [ "203"; "204"; "205"; "206" ]
        | ImplicitErrAddress // The address could not be classified due some errors.

        static member map =
            [
                ExplicitAddress, AddrInferenceType.ExplicitAddress
                ImplicitEvenOrOddAddress, AddrInferenceType.ImplicitEvenOrOddAddress
                ImplicitAllAddress, AddrInferenceType.ImplicitAllAddress
                ImplicitErrAddress, AddrInferenceType.ImplicitErrAddress
            ]

        member t.addrInferenceType =
            match t with
            | ExplicitAddress -> AddrInferenceType.ExplicitAddress
            | ImplicitEvenOrOddAddress -> AddrInferenceType.ImplicitEvenOrOddAddress
            | ImplicitAllAddress -> AddrInferenceType.ImplicitAllAddress
            | ImplicitErrAddress -> AddrInferenceType.ImplicitErrAddress

        static member tryCreate a = AddressInferenceType.map |> List.tryPick (fun (f, c) -> if c = a then Some f else None)


    type Address =
        {
            numberOpt : Number option
            streetOpt : Street option
            unitOpt : UnitNumber option
            cityOpt : City option
            stateOpt : State option
            zipCodeOpt : ZipCode option
            countyOpt : County option
            locationOpt : Location option
            fullAddressOpt : string option
            addressInferenceType : AddressInferenceType option
        }

        static member defaultValue : Address =
            {
                numberOpt = None
                streetOpt = None
                unitOpt = None
                cityOpt = None
                stateOpt = None
                zipCodeOpt = None
                countyOpt = None
                locationOpt = None
                fullAddressOpt = None
                addressInferenceType = None
            }

        member this.isEmpty = this = Address.defaultValue

        /// Allowed to have missing zip code.
        member this.isValid =
            match this.numberOpt, this.streetOpt, this.cityOpt, this.stateOpt with
            | Some _, Some _, Some _, Some _ -> true
            | _ -> false

        member this.isValidWithLocation =
            match this.locationOpt, this.isValid with
            | Some _, true -> true
            | _ -> false

        member this.asString (removeUnit : bool) =
            [|
                this.numberOpt |> asString
                this.streetOpt |> asString
                (if removeUnit then String.Empty else this.unitOpt |> asString)
                this.cityOpt |> asString
                (match this.stateOpt with | Some s -> stateFactory.getKey s | None -> String.Empty)
                this.zipCodeOpt |> asString
            |]
            |> Array.filter (fun e -> e <> String.Empty)
            |> seqToString

        member this.asOriginalString (removeUnit : bool) =
            [|
                this.numberOpt |> asString
                this.streetOpt |> asOriginalString
                (if removeUnit then String.Empty else this.unitOpt |> asString)
                this.cityOpt |> asOriginalString
                (match this.stateOpt with | Some s -> stateFactory.getKey s | None -> String.Empty)
                this.zipCodeOpt |> asString
            |]
            |> Array.filter (fun e -> e <> String.Empty)
            |> seqToString

        member this.streetCityStateOpt =
            match this.streetOpt, this.cityOpt, this.stateOpt with
            | Some s, Some c, Some st -> { street = s; city = c; state = st } |> Some
            | _ -> None


    type FullAddressRaw =
        | FullAddressRaw of AddressPartWithOriginal

        member this.value = let (FullAddressRaw v) = this in v.projectedName
        member this.originalValue = let (FullAddressRaw v) = this in getValueOrDefault v.projectedName v.originalName


    type StreetNameRaw =
        | StreetNameRaw of AddressPartWithOriginal

        member this.value = let (StreetNameRaw v) = this in v.projectedName
        member this.originalValue = let (StreetNameRaw v) = this in getValueOrDefault v.projectedName v.originalName


    type StreetZipKey =
        {
            City : string
            State : State
            ZipCode : string
            StreetFullName : string
        }


    type StreetZipInfo =
        {
            key : StreetZipKey
            OccurrenceCount : int
            StreetOriginalName : string
            CityOriginalName : string
        }


    type ZipCodeCityInfo =
        {
            ZipCode : string
            State : State
            City : string
            CityOriginalName : string
        }


    type StreetCityStateZip =
        {
            streetCityState : StreetCityState
            zipCode : ZipCode
        }

        member this.streetZipKey =
            {
                City = this.streetCityState.city.value
                State = this.streetCityState.state
                ZipCode = this.zipCode.value
                StreetFullName = this.streetCityState.street.value
            }

        member this.toStreetZipInfo() =
            {
                key = this.streetZipKey
                OccurrenceCount = 1
                StreetOriginalName = this.streetCityState.street.originalValue
                CityOriginalName = this.streetCityState.city.originalValue
            }

        static member fromStreetZipInfo (v : StreetZipInfo) =
            {
                streetCityState =
                    {
                        street = Street { projectedName = v.key.StreetFullName; originalName = Some v.StreetOriginalName }
                        city = City { projectedName = v.key.City; originalName = Some v.CityOriginalName }
                        state = v.key.State
                    }

                zipCode = ZipCode v.key.ZipCode
            }


    /// Structure to control how to un-project standard abbreviations into original words.
    type UnProjectInfo =
        {
            unprojectAll : bool
        }

        static member defaultValue =
            {
                unprojectAll = true
            }


    type DistanceFromPrev =
        | AddOne
        | AddTwo
        | AddOther


    /// Gets the distance from previous optional element of the list.
    /// The first [optional] element does not have previous element.
    let getDistanceFromPrev e po =
        match po with
        | Some p ->
            match e - p with
            | 1 -> AddOne
            | 2 -> AddTwo
            | _ -> AddOther
        | _ -> AddOther


    /// Gets getAddressInferenceType based on
    ///     1. d - distance from previous element
    ///     2. b - true if the element matches something in the original list.
    ///     3. t - optional AddressInferenceType of the previous element.
    let getAddressInferenceType d t b =
        match b with
        | true -> ExplicitAddress
        | false ->
            match d with
            | AddOne -> ImplicitAllAddress
            | AddTwo ->
                match t with
                | Some v ->
                    match v with
                    | ExplicitAddress | ImplicitEvenOrOddAddress -> ImplicitEvenOrOddAddress
                    | ImplicitAllAddress -> ImplicitAllAddress
                    | ImplicitErrAddress -> ImplicitErrAddress
                | None -> ImplicitAllAddress
            | AddOther -> ImplicitAllAddress


    let getInferenceTypes oi ei =
        let err () = ei |> List.map(fun (_, a) -> a, ImplicitErrAddress)

        match isValid oi, isValid2 ei with
        | true, true ->
            let (o, e) = demote oi, demote2 ei
            match o |> List.tryHead, e |> List.tryHead, o |> List.rev |> List.tryHead, e  |> List.rev |> List.tryHead with
            | Some i, Some (j, _), Some i1, Some (j1, _) when i = j && i1 = j1 ->
                // The lists must start from the same numbers and must end with the same numbers.
                let a = e |> List.fold (fun (acc, p) (i, v) -> (i, v, getDistanceFromPrev i p) :: acc, Some i) ([], None) |> fst |> List.rev

                let folder acc p (i, v, d) =
                    let t = getAddressInferenceType d p (o |> List.contains i)
                    (i, v, t) :: acc, Some t

                let b = a |> List.fold (fun (acc, p) r -> folder acc p r) ([], None) |> fst |> List.rev
                let c = b|> List.map (fun (_, v, d) -> (v, d))
                c
            | _ -> err ()
        | _ -> err ()


    /// Compares the Original addresses with Expanded ones
    /// and sets addressInferenceType in [the copy of] expanded addresses.
    let setAddressInferenceType oi ei =
        let tryGetNumber (n : Number option) =
            match n with
            | Some v ->
                match Int32.TryParse v.value with
                | (true, i) -> Some i
                | _ -> None
            | None -> None

        let group (v : list<Address>) = v |> List.map (fun a -> tryGetNumber a.numberOpt, a)|> List.groupBy (fun e -> (snd e).streetCityStateOpt)
        let (o, e) = group oi, group ei
        let m = o |> List.map (fun (a, b) -> a, b |> List.map fst) |> Map.ofList

        let x =
            e
            |> List.map (fun (st, r) -> getInferenceTypes (m.TryFind st |> getValueOrDefault []) r)
            |> List.concat
            |> List.map (fun (a, t) -> { a with addressInferenceType = Some t})

        x
