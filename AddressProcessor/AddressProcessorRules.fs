namespace Softellect.AddressProcessor

open StringParser
open CSharpInterop
open DatabaseTypes
open DataParsing
open AddressTypes
open Extensions
open MatchTypes
open MatchingRules
open DynamicData
open CityRules
open NewAddressRules
open StreetNameRules
open HouseNumberRules
open UnitNumberRules
open StateRules
open ZipCodeRules
open TryInferRules
open IfStreetNotFoundRules
open IfCityNotFoundRules
open MatchParams
open DynamicSql

open Microsoft.Data.SqlClient

module AddressProcessorRules =

    let maxIterations = 500


    /// kk:20200113 - At this point we have only one fuzzy search combination of parameters.
    /// This may change. If / when it does, remove this constant and then follow the red trail to see if more detailed information is useful upstream.
    [<Literal>]
    let FuzzySearchPerformed = "FuzzySearchPerformed"


    type ParseInfo =
        {
            connectionData : SqlConnectionData
            mapData : MapData
            request : AddressProcessorParseRequest
        }


    type MatchError
        with

        member this.addressProcessorResultError =
            match this with
            | ZipNotFound -> AddressProcessorResultError.ZipNotFound
            | StateNotFound -> AddressProcessorResultError.StateNotFound
            | CityNotFound -> AddressProcessorResultError.CityNotFound
            | StreetNotFound -> AddressProcessorResultError.StreetNotFound
            | InvalidHouseNumber -> AddressProcessorResultError.InvalidHouseNumber
            | InvalidUnitNumber -> AddressProcessorResultError.InvalidUnitNumber
            | InvalidAddress -> AddressProcessorResultError.InvalidAddress
            | TooLarge -> AddressProcessorResultError.TooLarge
            | DuplicateAddress -> AddressProcessorResultError.DuplicateAddress
            | HouseNumberNotFound -> AddressProcessorResultError.HouseNumberNotFound
            | CriticalError -> AddressProcessorResultError.CriticalError


    let getMapData getConn =
        {
            zipUpdater = zipMapUpdater getConn
            zipMetricUpdater = zipMetricMapUpdater getConn
            stateCityUpdater = stateCityMapUpdater getConn
            stateCityMetricUpdater = stateCityMetricMapUpdater getConn
            zipToCityUpdater = zipToCityMapUpdater getConn
            zipToCityMetricUpdater = zipToCityMetricMapUpdater getConn
            stateToCityUpdater = stateToCityMapUpdater getConn
            zipCodeSelectMapUpdater = zipCodeSelectMapUpdater getConn
            citySelectMapUpdater = citySelectMapUpdater getConn
            wordMap = loadMapWordCorrection getConn
        }


    let getRules mapData removeUnitNumber p =
        {
            mapData = mapData
            rules = []
            allRules =
                [
                    TryInferRule tryInferRule
                    ZipRule zipRule
                    StateRule stateRule
                    CityRule cityRule
                    UnitRule (if removeUnitNumber then unitIdRule else unitRule)
                    StreetRule streetRule
                    NumberRule numberRule
                    NewAddressRule newAddressRule
                ]

            newAddress = newAddressRule
            matchParams = p
        }.resetRules()


    let getStreetNotFoundRules mapData removeUnitNumber p =
        {
            mapData = mapData
            rules = []
            allRules =
                [
                    TryInferRule tryInferRule
                    ZipRule zipRule
                    StateRule stateRule
                    CityRule cityRule
                    UnitRule (if removeUnitNumber then unitIdRule else unitRule)
                    IfStreetNotFoundRule ifStreetNotFoundRule
                    NumberRule numberRule
                    NewAddressRule newAddressRule
                ]

            newAddress = newAddressRule
            matchParams = p
        }.resetRules()


    let getRulesNoZip mapData removeUnitNumber p =
        {
            mapData = mapData

            rules = []
            allRules =
                [
                    TryInferRule tryInferRule
                    ZipRule zipRuleNoZip
                    StateRule stateRule
                    CityRule cityRuleWithTrySetZip
                    UnitRule (if removeUnitNumber then unitIdRule else unitRule)
                    StreetRule streetRule
                    NumberRule numberRule
                    NewAddressRule newAddressRule
                ]

            newAddress = newAddressRule
            matchParams = p
        }.resetRules()


    let getRulesNoCity mapData removeUnitNumber p =
        {
            mapData = mapData
            rules = []
            allRules =
                [
                    TryInferRule tryInferRule
                    ZipRule zipRule
                    StateRule stateRule
                    CityRule cityRuleNoCity
                    UnitRule (if removeUnitNumber then unitIdRule else unitRule)
                    StreetRule streetRuleWithTrySetCity
                    NumberRule numberRule
                    NewAddressRule newAddressRule
                ]

            newAddress = newAddressRule
            matchParams = p
        }.resetRules()


    /// Rules to use when we don't have either street or city in the database for a given zip.
    let getRulesNoStreetNoCity mapData removeUnitNumber p =
        {
            mapData = mapData
            rules = []
            allRules =
                [
                    TryInferRule tryInferRule
                    ZipRule zipRule
                    StateRule stateRule
                    IfCityNotFoundRule ifCityNotFoundRule
                    UnitRule (if removeUnitNumber then unitIdRule else unitRule)
                    IfStreetNotFoundRule ifStreetNotFoundRule
                    NumberRule numberRule
                    NewAddressRule newAddressRule
                ]

            newAddress = newAddressRule
            matchParams = p
        }.resetRules()


    let getRulesSimple mapData p =
        {
            mapData = mapData
            rules = []

            allRules =
                [
                    TryInferRule tryInferRule
                    ZipRule zipRuleNoZip
                    StateRule stateRule
                    CityRule cityRule
                    UnitRule unitIdRule
                    StreetRule streetRule
                    NumberRule numberRule
                    NewAddressRule newAddressRule
                ]

            newAddress = newAddressRule
            matchParams = p
        }.resetRules()


    let getParseRules mapData (p : AddressProcessorInputParams) =
        let removeUnitNumber = p.RemoveUnitNumber
        let rulesMain = getRules mapData removeUnitNumber
        let rulesNoZip = getRulesNoZip mapData removeUnitNumber
//        let rulesSimple = getRulesSimple mapData
        let ruleStreetNotFound = getStreetNotFoundRules mapData removeUnitNumber
        let rulesNoCity = getRulesNoCity mapData removeUnitNumber
        let rulesNoStreetNoCity = getRulesNoStreetNoCity mapData removeUnitNumber

        let strictValue =
            match p.DoFuzzySearch with
            | false -> MatchParams.strictValue
            | true -> MatchParams.strictFuzzySearchValue

        let rules =
            match p.IgnoreStreetNotFound with
            | false ->
                match p.DoSimpleParse with
                | false ->
                    [
                        rulesMain strictValue
                        rulesMain MatchParams.mediumValue
                        rulesMain MatchParams.relaxedValue
                        rulesNoZip MatchParams.strictValue
                        rulesNoZip MatchParams.mediumValue
                        rulesNoZip MatchParams.relaxedValue
                        rulesNoCity MatchParams.strictSkippedCityValue

                    ]
                | true ->
                    [
//                        rulesSimple MatchParams.mediumValue
                        rulesNoStreetNoCity MatchParams.strictValue
                    ]
            | true ->
                match p.DoSimpleParse with
                | false ->
                    [
                        rulesMain MatchParams.strictValue
                        rulesMain MatchParams.mediumValue
                        rulesMain MatchParams.relaxedValue
                        rulesNoZip MatchParams.strictValue
                        rulesNoZip MatchParams.mediumValue
                        rulesNoZip MatchParams.relaxedValue
                        ruleStreetNotFound MatchParams.strictValue
                    ]
                | true ->
                    [
//                        rulesSimple MatchParams.mediumValue
                        ruleStreetNotFound MatchParams.strictValue
                        rulesNoStreetNoCity MatchParams.strictValue
                    ]

        rules


    /// Do not merge a, b, c, ... into a single statement. Once in a while we do need to examine them during debugging...
    let toInputList p s =
        let a = s |> standardize CleanStringParams.unitNumberValue // takes care of unit numbers like "1-F" -> "1F"
        let b = a |> combineIfNotTooLong None
        let c = b |> standardize p // Takes care of house number ranges like "123-5" -> "123 124 125"
        c


    let toInput p s =
        let c = toInputList p s
        let d = c |> List.rev
        let e = d |> MatchInfo.create
        e


    let cleanStreetLine s =
        s
        |> toInputList CleanStringParams.doNotExpandValue
        |> seqToString


    /// kk:20191212 - Note that this function will be different from the one above once word corrections are implemented.
    let cleanCity s =
        s
        |> toInputList CleanStringParams.doNotExpandValue
        |> seqToString


    //let getProjectedStreetLineImpl street = street |> cleanStreetLine
    //let getProjectedCityImpl city = city |> cleanCity


    let loadAddressKey (getCoreConn : unit -> SqlConnection) a b =
        use conn = new DynamicSqlConnection(getCoreConn)

        let streetLine =
            [
                a.numberOpt |> Option.bind (fun e -> Some e.value)
                a.streetOpt |> Option.bind (fun e -> Some e.value)
                a.unitOpt |> Option.bind (fun e -> if b then None else Some e.rawValue)
            ]
            |> List.choose id
            |> seqToString
            |> cleanStreetLine

        match a.cityOpt, a.stateOpt, a.zipCodeOpt with
        |  Some c, Some st, None ->
            use data = conn?<<addressKeyNoZipSelectStr
            data?StateCode <- st.key
            data?City <- c.value
            data?StreetLine <- streetLine
            use reader = data.ExecuteReader()
            [ while reader.Read() do yield reader?AddressKey.ToString() |> AddressKey.tryCreate ]
            |> List.choose id
            |> List.tryHead
        | Some c, Some st, Some z ->
            use data = conn?<<addressKeySelectStr
            data?Zip <- z.value
            data?StateCode <- st.key
            data?City <- c.value
            data?StreetLine <- streetLine
            use reader = data.ExecuteReader()
            [ while reader.Read() do yield reader?AddressKey.ToString() |> AddressKey.tryCreate ]
            |> List.choose id
            |> List.tryHead
        | _ -> None


    let tryGetAddressKey getCoreConn a b =
            try
                loadAddressKey getCoreConn a b
            with
            | ex ->
                logLoadingErr "loadAddressKey" ex
                None


    let rec doParse r m =
        match m.result, m.unprocessed with
        | Failed, _ -> m
        | _, [] -> m
        | _ ->
            match r with
            | [] -> m
            | h :: t ->
                let _, m1 = applyRules (h, m)

                match m1.result with
                | Perfect -> m1
                | Partial _ -> m1
                | Failed ->
                    match t with
                    | [] -> m1 // There are no more rules, so return the result of last rule.
                    | _ -> doParse t m


    let parseAddressImpl rules input =
        let rec inner c m =
            if c < maxIterations
            then
                match m.result, m.unprocessed with
                | Failed, _ | _, [] -> m
                | _ -> doParse rules m |> inner (c + 1)
            else
                { m with result = Failed; matchError = Some TooLarge }

        let output = input |> inner 0
        output


    let parseAddress mapData p r =
        let rules = getParseRules mapData r.ParseParams.InputParams
        let input = toInput p r.AddressString
        parseAddressImpl rules input


    let getFuzzySearchInfo i =
        match i.request.ParseParams.InputParams.DoFuzzySearch with
        | false -> EmptyString
        | true -> FuzzySearchPerformed


    let toAddressProcessorResult i (d, a) =
        let hasAddressKey, addressKey =
            match i.request.ParseParams.OutputParams.GetAddressKey with
            | false -> false, EmptyString
            | true ->
                match tryGetAddressKey i.connectionData.getCoreConn a i.request.ParseParams.InputParams.RemoveUnitNumber with
                | Some k -> true, k.value
                | None -> false, EmptyString

        let hasAddressInferenceType, addressInferenceType =
            let x = streetNameOrTypeWeight
            match i.request.ParseParams.OutputParams.GetAddressInferenceType with
            | false -> false, AddrInferenceType.None
            | true ->
                match a.addressInferenceType with
                | None -> false, AddrInferenceType.None
                | Some t -> true, t.addrInferenceType

        let matchError, status =
            match d.streetResult with
            | Matched | Inferred-> EmptyString, AddressProcessorResultError.ParsedSuccessfully
            | NotMatched -> StreetNotFound.ToString(), AddressProcessorResultError.StreetNotFound

        let fuzzySearchInfo = getFuzzySearchInfo i

        {
            Status = status
            HasAddressKey = hasAddressKey
            HasAddressInferenceType = hasAddressInferenceType
            MatchError = matchError
            Address = a
            ParsedAddress = a.asString i.request.ParseParams.InputParams.RemoveUnitNumber
            AddressKey = addressKey
            AddressInferenceType = addressInferenceType
            FuzzySearchInfo = fuzzySearchInfo
        }


    let toResolved m = m.resolved |> List.map (fun e -> (e.resolvedAddress, e.resolvedStepResults)) |> List.unzip


    let parseImpl i =
        let p =
            match i.request.ParseParams.InputParams.ExpandHyphen with
            | true -> CleanStringParams.defaultValue
            | false -> CleanStringParams.doNotExpandValue

        let m = parseAddress i.mapData p i.request

        let resolved =
            let e, r = toResolved m

            match i.request.ParseParams.OutputParams.GetAddressInferenceType with
            | false -> e
            | true ->
                let a = parseAddress i.mapData CleanStringParams.doNotExpandValue i.request
                let o, _ = toResolved a
                let t = setAddressInferenceType o e
                t
            |> List.zip r

        let a = resolved |> List.map (fun x -> toAddressProcessorResult i x)
        let fuzzySearchInfo = getFuzzySearchInfo i

        let failed =
            match m.matchError with
            | None -> None
            | Some e ->
                {
                    Status = e.addressProcessorResultError
                    HasAddressKey = false
                    HasAddressInferenceType = false
                    MatchError = e.ToString()
                    Address = Address.defaultValue
                    ParsedAddress = EmptyString
                    AddressKey = EmptyString
                    AddressInferenceType = AddrInferenceType.None
                    FuzzySearchInfo = fuzzySearchInfo
                }
                |> Some

        let retVal =
            match failed with
            | None -> a
            | Some f -> f :: a
            |> Array.ofList

        retVal
