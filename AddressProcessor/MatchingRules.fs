namespace Softellect.AddressProcessor

open AddressTypes
open MatchTypes
open Extensions
open DataUpdater
open DynamicData
open StringParser
open MatchParams
open MetricTreeInterop

module MatchingRules =

    /// "Standard" maximum number or words to skip.
    [<Literal>]
    let MaxSkipLength = 3


    /// Maximum number or words to match.
    [<Literal>]
    let MaxMatchLength = 7


    /// Default match threshold.
    [<Literal>]
    //let MatchThreshold = 0.5001
    let MatchThreshold = 0.6401


    /// More stringent match threshold to rule out partial street name matches when matching multiple house numbers AFTER the first address.
    [<Literal>]
    //let HouseNumberInferredThreshold = 0.3601
    let HouseNumberInferredThreshold = 0.4260


    /// Maximum number of words to USE when creating house number.
    let maxNumberMatchLength = Some 4


    /// Maximum number of words to SKIP when creating house number.
    let maxNumberSkipLength = Some 2


    /// Maximum number of words to USE when creating unit number.
    let maxUnitNumberMatchLength = Some 2


    /// Maximum number of words to SKIP when creating unit number.
    let maxUnitNumberSkipLength = Some 0


    /// We must have some positive minimum norm to guarantee that division by 0 won't happen. Here it is.
    [<Literal>]
    let MinNorm = 0.001


    /// Default value of a010 parameter in adjustMatchResult function.
    [<Literal>]
    let SkipWeightParA010 = 0.0700


    /// Parameters to control adjustMatchResult function f(p, n, m):
    ///     f(p, n, m) = p + a010 * n + a110 * p * n + ...)
    /// where n is the number of skipped words and m is maximum number of skipped words.
    /// Alternatively we can just use a look up dictionary to perform adjustments. Update as needed.
    type AdjustMatchResultInfo =
        {
            maxSkip : int // anything beyond that should result in Failure.
            skipWeightParA010 : float option // Parameter a01 in the Taylor expansion above.
        }

        static member getDefault m =
            {
                maxSkip = m
                skipWeightParA010 = None
            }


    /// Adjusts MatchResult based on how many words were skipped.
    let adjustMatchResult i (skip : list<string>) r =
        let n = skip |> List.map getSkipWeight |> List.sum
        let a010 = getValueOrDefault SkipWeightParA010 i.skipWeightParA010

        let adjust p =
            if n <= float i.maxSkip
            then
                let x = p + a010 * n
                Partial x
            else Failed

        match r with
        | Failed -> Failed
        | Partial p -> adjust p
        | Perfect ->
            match skip.Length with
            | 0 -> Perfect
            | _ -> adjust 0.0


    /// We need to sort the list for exact search.
    let toTryFind (x : Map<list<'A>, 'B> option) = x |> Option.bind (fun m -> (fun k -> k |> List.sort |> m.TryFind) |> Some)


    /// kk:20200211 - Do not delete commented line. It is used to switch between MetricListMap and ProjectedMap.
    /// We need to reverse the list for fuzzy search because we parse the string backwards.
    let toTryFuzzyFind d (x : MetricListMap<'B> option) = x |> Option.bind (fun m -> (fun k -> k |> List.rev |> concat |> m.tryFind d |> Some) |> Some)
    //let toTryFuzzyFind d (x : ProjectedMap<'B> option) = x |> Option.bind (fun m -> (fun k -> k |> List.rev |> concat |> m.tryFind d |> Some) |> Some)


    /// All maps used by the system.
    type MapData =
        {
            zipUpdater : AsyncUpdater<ZipCode, ZipMap>
            zipMetricUpdater : AsyncUpdater<ZipCode, ZipMetricMap>
            stateCityUpdater : AsyncUpdater<State * City, StateCityMap>
            stateCityMetricUpdater :  AsyncUpdater<State * City, StateCityMetricMap>
            zipToCityUpdater : AsyncUpdater<unit, ZipToCityMap>
            zipToCityMetricUpdater : AsyncUpdater<unit, ZipToCityMetricMap>
            stateToCityUpdater : AsyncUpdater<unit, StateToCityMap>
            zipCodeSelectMapUpdater : AsyncUpdater<StreetCityState, ZipCodeSelectMap>
            citySelectMapUpdater : AsyncUpdater<StreetCityState * ZipCode, CitySelectMap>
            wordMap : Map<ZipCode, Map<string, string>>
        }


    type RuleFunc = (RuleInfo * MatchInfo) -> (RuleInfo * MatchInfo)


    and Rule =
        | ZipRule of RuleFunc
        | StateRule of RuleFunc
        | CityRule of RuleFunc
        | UnitRule of RuleFunc
        | StreetRule of RuleFunc
        | NumberRule of RuleFunc
        | NewAddressRule of RuleFunc
        | TryInferRule of RuleFunc
        | IfStreetNotFoundRule of RuleFunc
        | IfCityNotFoundRule of RuleFunc

        member this.apply ri =
            match this with
            | ZipRule r | StateRule r | CityRule r | UnitRule r | StreetRule r | NumberRule r | NewAddressRule r | TryInferRule r | IfStreetNotFoundRule r | IfCityNotFoundRule r ->
                r ri


    and RuleInfo =
        {
            mapData : MapData
            rules : list<Rule>    // Current not yet processed rules.
            allRules : list<Rule> // All rules.
            newAddress : RuleFunc
            matchParams : MatchParams
        }

        with
            member this.tryFind (z : ZipCode) =
                match this.matchParams.fuzzySearchParams.streetSearchParams.streetSearchType with
                | ExactSearch ->
                    this.mapData.zipUpdater.updateContent z
                    //this.mapData.zipUpdater.getContent().TryFind z |> toTryFind
                    let a = this.mapData.zipUpdater.getContent()
                    let b = a.TryFind z
                    let c = b |> toTryFind
                    c
                | FuzzySearch ->
                    this.mapData.zipMetricUpdater.updateContent z
                    this.mapData.zipMetricUpdater.getContent().TryFind z
                    |> toTryFuzzyFind this.matchParams.fuzzySearchParams.streetDistance

            member this.tryFind (s : State, c : City) =
                match this.matchParams.fuzzySearchParams.streetSearchParams.streetSearchType with
                | ExactSearch ->
                    this.mapData.stateCityUpdater.updateContent (s, c)
                    this.mapData.stateCityUpdater.getContent().TryFind (s, c) |> toTryFind
                | FuzzySearch ->
                    this.mapData.stateCityMetricUpdater.updateContent (s, c)
                    this.mapData.stateCityMetricUpdater.getContent().TryFind (s, c)
                    |> toTryFuzzyFind this.matchParams.fuzzySearchParams.streetDistance

            member this.tryFindCity (z : ZipCode) =
                match this.matchParams.fuzzySearchParams.citySearchParams.citySearchType with
                | ExactSearch ->
                    this.mapData.zipToCityUpdater.updateContent()
                    this.mapData.zipToCityUpdater.getContent().TryFind z |> toTryFind
                | FuzzySearch ->
                    this.mapData.zipToCityMetricUpdater.updateContent()
                    this.mapData.zipToCityMetricUpdater.getContent().TryFind z
                    |> toTryFuzzyFind this.matchParams.fuzzySearchParams.cityDistance

            member this.tryFindCity (s : State) =
                this.mapData.stateToCityUpdater.updateContent()
                this.mapData.stateToCityUpdater.getContent().TryFind s |> toTryFind

            member this.getWordMap (z : ZipCode) =
                match this.mapData.wordMap.TryFind z with
                | Some x -> x
                | None -> Map.empty

            member this.resetRules() = { this with rules = this.allRules }
            member this.completed() = { this with rules = [] }


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


    let emptyRule (_ : RuleInfo) (m : MatchInfo) = m


    type FullComparandType =
        | NoSkippedTail      // Skipped tail is NOT included in full comparand. This is a default - use this if in doubt.
        | IncludeSkippedTail // Skipped tail IS included in full comparand.
        | OnlySkippedTail    // It is ONLY a skipped tail.


    type BestMatchComparandData<'B> =
        {
            // Words that we will try to compare with some values.
            comparand : 'B

            // The whole set to compare. The meaning depends on fullComparandType.
            fullComparand : 'B

            fullComparandType : FullComparandType

            // Skipped words.
            skipped : 'B
        }


    type StreetValidationInputData =
        {
            streetValidationCityOpt : City option // need a different label so that F# type inference won't go crazy.
            fuzzySearchParams : FuzzySearchParams
        }


    type BestMatchInputData<'A, 'B> =
        {
            values : list<'A>
            comparandData : BestMatchComparandData<'B>
        }


    type BestMatchData<'A, 'B> =
        {
            bestMatchInputData : BestMatchInputData<'A, 'B>
            validator : BestMatchComparandData<'B> -> 'A -> MatchResult
        }


    /// Finds the best match of b over the list of 'A using a validator v.
    /// Returns Some <best match> if found or None if not.
    let tryGetBestMatch (d : BestMatchData<'A, 'B>) : (MatchResult * 'A) option =
        let rec inner (s : list<'A>) acc =
            match s with
            | [] -> acc // We want to return everything that was found before...
            | h :: t ->
                let o = d.validator d.bestMatchInputData.comparandData h

                match o with
                | MatchResult.Perfect ->
                    [ (MatchResult.Perfect, h) ]
                | MatchResult.Partial __ ->
                    inner t ((o, h) :: acc)
                | MatchResult.Failed ->
                    inner t acc

        let retVal =
            let x = inner d.bestMatchInputData.values []
            let matches = x |> List.sortBy (fun (r, _) -> r)
            matches |> List.tryHead

        retVal


    type FindMatchInputData<'A, 'B> =
        {
            input : list<string>
            correctWords : list<string> -> list<string>
            tryGet : list<string> -> list<'A> option
            validator : BestMatchComparandData<'B> -> 'A -> MatchResult
            maxMatchLength : int option
            maxSkipLength : int option
            matchThreshold : float option
            fullComparandType : FullComparandType
            toSublistsParams : ToSublistsParams
        }


    type FindMatchData<'A> =
        {
            value : 'A
            remainingOutput : list<string>
            skipped : list<string>
        }


    type MatchInputData =
        {
            matchComparandData : BestMatchComparandData<list<string>> // kk: Note the usage of differnt label here to simplify type inferrence.
            remainingInput : list<string>
            skipped : list<string>
        }


    type MatchData<'A> =
        {
            matchResult : MatchResult
            data : FindMatchData<'A>
        }


    /// Tries to get the best match from the beginning of the list and possibly skipping some words in the MIDDLE.
    let tryFindMatchImpl (f : FindMatchInputData<'A, list<string>>) (fi : list<string> option) =
        let maxMatchLength = getValueOrDefault MaxMatchLength f.maxMatchLength
        let matchThreshold = getValueOrDefault MatchThreshold f.matchThreshold

        let getMatchData g (r, a) =
            {
                matchResult = r

                data =
                    {
                        value = a
                        remainingOutput = g.remainingInput
                        skipped = g.skipped
                    }
            }
            |> Some

        let tryMatch (g : MatchInputData) =
            // Use sorted list as a key and unsorted list for everything else.
            // Sorting is done by the client.
            let k = g.matchComparandData.comparand

            match f.tryGet k with
            | Some x ->
                let current =
                    {
                        bestMatchInputData =
                            {
                                values = x
                                comparandData =
                                    match fi with
                                    | Some i -> { g.matchComparandData with fullComparand = i }
                                    | None -> g.matchComparandData
                            }

                        validator = f.validator
                    }
                    |> tryGetBestMatch
                    |> Option.bind (getMatchData g)
                current
            | None ->
                None

        /// Gets words, which are in k0 but not in e.
        /// Note that current implementation will remove repetitions.
        /// Revisit if necessary.
        let getSkipped k0 e =
            let b = e |> Set.ofList
            let skipped = k0 |> List.filter (fun a -> b.Contains a |> not)
            skipped

        /// "ST GEORGE ST" in LA will produce perfect match with both "GEORGE ST" and "ST GEORGE ST" and no skip.
        /// However, match by "ST GEORGE ST" will consume more words, thus remainingOutput will have smaller length.
        /// These streets do exist in LA and with wrong ZIP we won't be able to distinguish them.
        let sorter e = (e.matchResult, e.data.skipped.Length, e.data.remainingOutput.Length)

        let getBestMatch (i : MatchInputData) =
            let k0 = i.matchComparandData.comparand |> f.correctWords
            let a = k0 |> toValidSubListsWithMatchingEnds f.toSublistsParams |> Set.toList
            let getSkipped e = (getSkipped k0 e) @ i.skipped

            let r =
                a
                |> List.map (fun e -> tryMatch { matchComparandData = { i.matchComparandData with comparand = e; skipped = getSkipped e } ; remainingInput = i.remainingInput; skipped = getSkipped e })
                |> List.choose id
                |> List.sortBy sorter

            r |> List.tryHead

        let subLists = [ 1..(min f.input.Length maxMatchLength) ] |> List.map (fun i -> f.input |> List.splitAt i)

        // Note that we send downstream a "perfect" comparand (== the same as a fullComparand).
        // However, there we will try shorter (== "not perfect") comparands.
        let r0 =
            subLists
            |> List.map (fun (used, remaining) -> getBestMatch
                                                    {
                                                        matchComparandData =
                                                            {
                                                                comparand = used
                                                                fullComparand = used
                                                                fullComparandType = f.fullComparandType
                                                                skipped = []
                                                            }

                                                        remainingInput = remaining
                                                        skipped = []
                                                    })

        let r =
            r0
            |> List.choose id
            |> List.sortBy sorter

        match r |> List.tryHead with
        | Some v ->
            match v.matchResult with
            | Perfect -> Some v
            | Partial p when p <= matchThreshold -> Some v
            | _ -> None
        | None -> None


    /// Tries to pick the best match while allowing some words to be skipped from the BEGINNING of the list.
    let tryPickMatchImpl (f : FindMatchInputData<'A, list<string>>) =
        let maxSkipLength = getValueOrDefault MaxSkipLength f.maxSkipLength
        let failed = None, MatchResult.Failed
        let adjustMatchResult = adjustMatchResult (AdjustMatchResultInfo.getDefault maxSkipLength)

        let rec inner skip tail =
            match tail with
            | [] -> None, MatchResult.Failed
            | h :: t ->
                let retry() = inner (h :: skip) t
                let tryRetry() = if skip.Length < maxSkipLength then retry () else failed

                let i =
                    match f.fullComparandType with
                    | IncludeSkippedTail -> Some ((h :: skip) |> List.rev)
                    | NoSkippedTail -> None
                    | OnlySkippedTail -> Some (skip |> List.rev)

                match tryFindMatchImpl { f with input = tail } i with
                | Some e ->
                    let x = Some { e.data with skipped = e.data.skipped @ skip }, e.matchResult |> (adjustMatchResult skip)
                    let y = tryRetry()

                    [ x; y ]
                    |> List.sortBy (fun (_, r) -> r)
                    |> List.tryHead
                    |> (getValueOrDefault failed)
                | None -> tryRetry()

        inner [] f.input


    /// TODO kk:20200114 - If we find the need to set toSublistsParams from higher level code, refactor this function to take LESS parameters.
    /// Tries to find the best matching street / city from the beginning of the list (y).
    /// w - correction dictionary.
    /// m - main map - finds all 'A by the list of words.
    /// y - input list
    /// v - validator
    let tryPickMatch
        (w : Map<string, string>)
        (tryFind : list<string> -> list<'A> option)
        (y : list<string>)
        (v : BestMatchComparandData<list<string>> -> 'A -> MatchResult) =
        {
            input = y
            correctWords = correctWords w
            tryGet = tryFind
            validator = v
            maxMatchLength = None
            maxSkipLength = None
            matchThreshold = None
            fullComparandType = NoSkippedTail
            toSublistsParams = ToSublistsParams.fuzzyMatchValue
        }
        |> tryPickMatchImpl


    /// Tries to get house number while allowing some words to be skipped from the BEGINNING of the list.
    /// One of the challenges is shown in the following example. Consider two sub strings of an address:
    ///     "... 25 Main, 37 St George ..."
    ///     "... 25 & Garage, 37 St George ..."
    /// Note that both don't have a street type.
    /// The second example has a "garbage" word, which we can skip, but the first one is not because it matches street name
    /// We need to "turn on" full comparand (without skipping the words from the end) so that the validator could try matching the street name.
    let tryPickHouseNumber y v =
        {
            input = y
            correctWords = id
            tryGet = fun e -> e |> List.rev |> Number.tryCreate |> Option.bind (fun e -> Some [ e ])
            validator = v
            maxMatchLength = maxNumberMatchLength
            maxSkipLength = maxNumberSkipLength
            matchThreshold = None
            fullComparandType = OnlySkippedTail
            toSublistsParams = ToSublistsParams.wordsOnlyValue
        }
        |> tryPickMatchImpl


    /// Tries to get unit number from the beginning of the list.
    let tryPickUnitNumber y v =
        {
            input = y
            correctWords = id
            tryGet = fun e -> e |> List.rev |> UnitNumber.tryCreate |> Option.bind (fun e -> Some [ e ])
            validator = v
            maxMatchLength = maxUnitNumberMatchLength
            maxSkipLength = maxUnitNumberSkipLength
            matchThreshold = None
            fullComparandType = OnlySkippedTail
            toSublistsParams = ToSublistsParams.wordsOnlyValue
        }
        |> tryPickMatchImpl


    /// Using word weight function w, compares that two lists of strings (a and b) match.
    let compareLists (w : string -> float) (a : list<string>) (b : list<string>) : MatchResult =
        //match (a |> List.sort) = (b |> List.sort) with
        match a = b with
        | true -> MatchResult.Perfect
        | false ->
            // "ST GEORGE ST" is very similar to "GEORGE ST" (without the Saint) or to "ST GEORGE" (without the street).
            let sa = a |> Set.ofList
            let sb = b |> Set.ofList

            // TODO kk:20191113 - Change to comparing how many permutations are needed to transform one list into another.
            let i = Set.intersect sa sb

            match i.IsEmpty with
            | false ->
                // !!! Note that we cannot use Set.map and then Set.fold here because Set.map will squash the same values into just one !!!
                let getNorm s = s |> Set.toList |> List.map w |> List.fold (fun acc r -> acc + r) 0.0

                let norm =
                    let na = getNorm sa
                    let nb = getNorm sb
                    max MinNorm (max na nb)

                // List.sort is ascending, which means that full set match should have a value of 0.0.
                let retVal = MatchResult.Partial (max ((norm - (getNorm i)) / norm) 0.0)
                retVal
            | true -> Partial 1.0


    type HouseNumberValidatorInputData =
        {
            streetCityStates : list<string> -> list<StreetCityState> option
            wordMap : Map<string, string>
            addressData : Address option
            previous : DetailedStepResult
            fuzzySearchParams : FuzzySearchParams
        }

        member d.toStreetValidationInputData c =
            {
                streetValidationCityOpt = c
                fuzzySearchParams = d.fuzzySearchParams
            }


    let getMaps (ri : RuleInfo) a =
        match a.zipCodeOpt, a.stateOpt, a.cityOpt with
        | Some z, _, _ -> ri.tryFind z, ri.getWordMap z
        | _, Some s, Some c -> ri.tryFind (s, c), Map.empty
        | _ -> None, Map.empty
        |> (fun (a, b) -> getValueOrDefault (fun _ -> None) a, b)


    // Tries to skip n words. Returns None if this is not possible.
    let trySkip n m =
        let rec inner i g =
            if i < 0 then None
            else
                match i with
                | 0 -> Some g
                | _ ->
                    match g.unprocessed with
                    | [] -> None
                    | h :: t -> inner (i - 1) { g with currentSkipped = h :: g.currentSkipped; unprocessed = t }

        inner n m


    let defaultSorter p i (_, m) =
        let failed, matchScore =
            match m.result with
            | Perfect -> 0, 0.00
            | Partial x -> 0, x * p.skipParams.matchWeight
            | Failed -> 1, 1_000.0

        let x =
            [
                (float m.unprocessed.Length)
                (float (m.currentSkipped.Length + m.allSkipped.Length)) * p.skipParams.skippedWordWeight
                matchScore
            ]

        let a = failed, ((x |> List.sum) * p.skipParams.roundingMultiplier |> int |> decimal) / (decimal p.skipParams.roundingMultiplier), i
        a


    /// Tries to get the best result ouf ot rule collection results
    /// or applies default value getter in case the collection is empty.
    let tryRuleCollectionHead f rc =
        let x = rc |> List.tryHead |> Option.bind (fun (_, e) -> Some e)
        getValueOrDefault2 f x


    /// !!! kk:20191125 - When in trouble - temporarily COPY the whole function as an inner function into the function
    ///     where there is a trouble, e.g. CityRules.tryAfterCity. Don't forget to delete once done!
    ///
    /// Processes a bound rule collection.
    /// Then sorts results using a given sorter.
    /// Then tries to get a head of the list.
    /// Returns failure if there is none (best match).
    let processBoundRuleCollection f s rc =
        let a =
            rc
            |> List.choose (fun (r, e) -> e |> Option.bind (fun v -> Some (r, v)))
            |> List.mapi(fun i e -> i, applyRules e)
            |> List.map(fun (i, m) -> s i m, m)
            |> List.sortBy(fun (i, _)-> i)

        let b = a |> tryRuleCollectionHead f
        b


    /// Binds the collection to rules (applies CURRENT rule), then processes the rest for each element of the list.
    let processRuleCollection f s (ri, m) rc =
        let a = rc |> List.map (fun e -> e (ri, m)) |> List.map (fun (a, b) -> a, Some b)
        let b = a |> processBoundRuleCollection (fun () -> f m) s
        b


    let removeUnitNumber r = { r with rules = r.rules |> List.filter (fun e -> match e with | UnitRule _ -> false | _ -> true)}
    let removeHouseNumber r = { r with rules = r.rules |> List.filter (fun e -> match e with | NumberRule _ -> false | _ -> true)}


    let tryFromResolved f p m =
        match m.resolved with
        | [] -> f m
        | h :: _ -> p h.resolvedAddress m


    let updateMatchInfo (d : FindMatchData<'A>) m u =
        { m with
            address = u m.address
            currentSkipped = m.currentSkipped |> List.append d.skipped
            unprocessed = d.remainingOutput
            matchError = None
        }


    let failed (ri : RuleInfo, m) = ri.completed(), m
