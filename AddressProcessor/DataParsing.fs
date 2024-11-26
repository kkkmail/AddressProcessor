namespace Softellect.AddressProcessor

open Microsoft.FSharp.Collections
open FSharp.Collections.ParallelSeq
open StringParser
open DatabaseTypes
open DynamicSql
open AddressTypes
open UnionFactories
open Extensions
open Errors
open Configuration
open Newtonsoft.Json

open Microsoft.Data.SqlClient

module DataParsing =

    let cleanStringParams = CleanStringParams.defaultValue


    type MergeAction =
        | InsertedRow
        | UpdatedRow


        static member tryCreate (s : string) =
            match s.ToUpper() with
            | "INSERT" -> Some InsertedRow
            | "UPDATE" -> Some UpdatedRow
            | _ -> None


    let private toDbError f i = i |> f |> DbErr |> Error
    let private mapException e = e |> DbException |> DbErr
    let toSettingError e = e |> SettingErr |> Error


    let private tryDbFun f g =
        try
            g()
        with
        | e -> f + (mapException e) |> Error


    let private trySettingDbFun f g = tryDbFun (SettingErr f) g
    let private tryQuotePropertyDbFun f g = tryDbFun (QuotePropertyErr f) g
    let getOpenConnection (getConn : unit -> SqlConnection) = getConn () |> toOpenConn


    let private loadStreetZip (reader : DynamicSqlDataReader) =
        {
            Id = reader?StreetZipId
            City = reader?City
            State = reader?StateCode
            ZipCode = reader?Zip
            OccurrenceCount = reader?OccurrenceCount
            StreetFullName = reader?StreetFullName
            StreetOriginalName = reader?StreetOriginalName
            CityOriginalName = reader?CityOriginalName
        }


    let loadStreetZipTblForZip (RatingConnectionGetter getConn) (z : ZipCode) =
        use conn = new DynamicSqlConnection(getConn)

        let r =
            seq {
                use data = conn?<<streetZipSelectStr
                data?Zip <- z.value
                use reader = data.ExecuteReader()
                while reader.Read() do yield loadStreetZip reader }
            |> List.ofSeq

        let r1 =
            seq {
                use data = conn?<<streetZipAddOnSelectStr
                data?Zip <- z.value
                use reader = data.ExecuteReader()
                while reader.Read() do yield loadStreetZip reader }
            |> List.ofSeq

        let retVal = Seq.append r r1
        retVal


    let loadStreetZipTblForStateCity (RatingConnectionGetter getConn) (s : State) (c : City) =
        use conn = new DynamicSqlConnection(getConn)

        let r =
            seq {
                use data = conn?<<streetZipStateCitySelectStr
                data?StateCode <- s.key
                data?City <- c.value
                use reader = data.ExecuteReader()
                while reader.Read() do yield loadStreetZip reader }
            |> List.ofSeq

        let r1 =
            seq {
                use data = conn?<<streetZipAddOnStateCitySelectStr
                data?StateCode <- s.key
                data?City <- c.value
                use reader = data.ExecuteReader()
                while reader.Read() do yield loadStreetZip reader }
            |> List.ofSeq

        let retVal = Seq.append r r1
        retVal


    let loadZipSelectTbl (RatingConnectionGetter getConn) (t : StreetCityState) =
        let retVal =
            seq {
                use conn = new DynamicSqlConnection(getConn)
                use data = conn?<<zipCodeSelectStr
                data?Street <- t.street.value
                data?City <- t.city.value
                data?StateCode <- t.state.key
                use reader = data.ExecuteReader()
                while reader.Read() do yield reader?Zip |> ZipCode.tryCreate }
            |> List.ofSeq
            |> List.choose id

        retVal


    let loadCitySelectTbl (RatingConnectionGetter getConn) (t : StreetCityState, z : ZipCode) =
        let retVal =
            seq {
                use conn = new DynamicSqlConnection(getConn)
                use data = conn?<<citySelectStr
                data?Street <- t.street.value
                data?StateCode <- t.state.key
                data?Zip <- z.value
                use reader = data.ExecuteReader()
                while reader.Read() do yield (reader?City, Some reader?CityOriginalName) |> City.tryCreate CleanStringParams.defaultValue }
            |> List.ofSeq
            |> List.choose id

        retVal


    let loadZipCodeCities (RatingConnectionGetter getConn) =
        let retVal =
            seq {
                use conn = new DynamicSqlConnection(getConn)
                use data = conn?<<zipCodeCitySelectStr
                use reader = data.ExecuteReader()

                while reader.Read()
                    do yield
                        {|
                            ZipCode = string reader?Zip
                            City = string reader?City
                            CityOriginalName = string reader?CityOriginalName
                            StateCode = string reader?StateCode
                        |} }
            |> List.ofSeq

        retVal


    let loadCounties (RatingConnectionGetter getConn) =
        let retVal =
            seq {
                use conn = new DynamicSqlConnection(getConn)
                use data = conn?<<countySelectStr
                use reader = data.ExecuteReader()

                while reader.Read()
                    do yield
                        {|
                            Id = int reader?CountyCode
                            CountyName = string reader?CountyName
                            StateCode = string reader?StateCode
                        |} }
            |> List.ofSeq

        retVal


    let mapStreetCityState t (g : #seq<StreetCityState>) =
        let toValidSortedSubLists (x : Street) = x.toValidSortedSubLists
        let toValidOriginalSortedSubLists (x : Street) = x.toValidOriginalSortedSubLists
        let w f = g |> Seq.map (fun x -> (f x.street t |> Set.toSeq |> Seq.map (fun e -> e, x)))

        match t.projectionUsage with
        | ProjectedOnly -> [ toValidSortedSubLists ]
        | OriginalOnly -> [ toValidOriginalSortedSubLists ]
        | ProjectedAndOriginal -> [ toValidSortedSubLists; toValidOriginalSortedSubLists ]
        |> List.map w
        |> List.fold Seq.append Seq.empty
        |> Seq.fold (fun acc r -> acc |> Seq.append r) Seq.empty
        |> Seq.groupBy (fun (e, _) -> e)
        |> Seq.map (fun (e, v) -> e, v |> List.ofSeq |> List.map (fun (_, s) -> s) |> List.distinct)
        |> Map.ofSeq


    // TODO (does not yet do what is described below).
    // TODO (2) - The same must be applied to streets.
    // Maps seq<City> into a map where:
    //    Key is the list of (some) sorted words in the city name.
    //    Value is the list of cities, which have these words, sorted in the order of decreasing match score (to be defined).
    let mapCity t (g : #seq<City>) =
        g
        |> PSeq.map (fun x -> (x.toValidSortedSubLists t |> Set.toSeq |> Seq.map (fun e -> e, x)))
        |> PSeq.fold (fun acc r -> acc |> Seq.append r) Seq.empty
        |> Seq.groupBy (fun (e, _) -> e)
        |> Seq.map (fun (e, v) -> e, v |> List.ofSeq |> List.map (fun (_, s) -> s))
        |> Map.ofSeq


    let processStreetZipTbl (streetZipTbl : seq<StreetZipDetailed>) =
        let retVal =
            streetZipTbl
            |> Seq.choose (fun r ->
                            let a() = r.ZipCode |> ZipCode.tryCreate
                            let b() = r |> Street.tryCreateCleanedFromStreetZipDetailed cleanStringParams
                            let c() = (r.City, r.CityOriginalName) |> City.tryCreate cleanStringParams
                            let d() = r.State |> stateFactory.tryFromKey

                            match a(), b(), c(), d() with
                            | Some z, Some s, Some c, Some t ->
                                Some (z, ({ street = s; city = c; state = t }, r.OccurrenceCount))
                            | _ -> None
                            )
            |> List.ofSeq

        retVal


    let toZipSeq t a =
        a
        |> Seq.groupBy (fun (z, _) -> z)
        |> Seq.map (fun (z, g) -> z, g |> Seq.map (fun (_, x) -> x) |> mapStreetCityState t)


    let loadMapWordCorrection (RatingConnectionGetter getConn) =
        seq {
            use conn = new DynamicSqlConnection(getConn)
            use data = conn?<<wordCorrectionSelectStr
            use reader = data.ExecuteReader()

            while reader.Read()
                do yield
                    {|
                        ZipCode = string reader?Zip
                        Wrong = string reader?Wrong
                        Correct = string reader?Correct
                    |} }
        |> Seq.choose (fun r ->
                        match r.ZipCode |> ZipCode.tryCreate with
                        | Some z -> Some (z, (r.Wrong, r.Correct))
                        | None -> None)
        |> Seq.groupBy (fun (z, _) -> z)
        |> Seq.map (fun (z, e) -> z, e |> Seq.map (fun (_, x) -> x) |> Map.ofSeq)
        |> Map.ofSeq


    let tryLoadSetting<'T> (SettingId settingId) (SettingVersion settingVersion) (RatingConnectionGetter getConn) =
        let g() =
            let s =
                seq {
                    use conn = new DynamicSqlConnection(getConn)
                    use data = conn?<<settingSelectStr
                    data?AddressProcessorSettingId <- settingId
                    use reader = data.ExecuteReader()
                    while reader.Read()
                        do yield
                            {|
                                SettingId = int reader?AddressProcessorSettingId
                                SettingVersion = int reader?SettingVersion
                                SettingJson = string reader?SettingJson
                            |} }
                |> Seq.tryFind (fun e -> e.SettingId = settingId)

            match s with
            | Some r ->
                match r.SettingVersion = settingVersion with
                | true ->
                    let a = r.SettingJson |> JsonConvert.DeserializeObject<'T>
                    a |> Some |> Ok
                | false -> { currentVersion = settingVersion; databaseVersion = r.SettingVersion } |> InvalidVersionNumberErr |> toSettingError
            | None -> Ok None

        trySettingDbFun SettingExn g


    let tryUpsertSetting<'T> (SettingId settingId) (SettingVersion settingVersion) (RatingConnectionGetter getConn) (q : 'T) =
        let g() =
            use conn = new DynamicSqlConnection(getConn)

            let cmdText = @"
                merge EFAddressProcessorSettings as target
                using (select @AddressProcessorSettingId, @settingVersion, @settingJson) as source (AddressProcessorSettingId, settingVersion, settingJson)
                on (target.AddressProcessorSettingId = source.AddressProcessorSettingId)
                when not matched then
                    insert (AddressProcessorSettingId, settingVersion, settingJson)
                    values (source.AddressProcessorSettingId, source.settingVersion, source.settingJson)
                when matched then
                    update set settingVersion = source.settingVersion, settingJson = source.settingJson;"

            use data = conn?<<cmdText
            data?AddressProcessorSettingId <- settingId
            data?settingVersion <- settingVersion
            data?settingJson <- q |> JsonConvert.SerializeObject
            let result = data.ExecuteNonQuery()

            match result = 1 with
            | true -> Ok()
            | false -> toDbError UpsertSettingErr settingId

        trySettingDbFun UpsertSettingExn g


    let tryLoadQuotePropertyProcessorSetting =
        tryLoadSetting<QuotePropertyProcessorSetting> SettingId.quotePropertyProcessorSettingId SettingVersion.quotePropertyProcessorSettingVersion


    let tryUpsertQuotePropertyProcessorSetting =
        tryUpsertSetting<QuotePropertyProcessorSetting> SettingId.quotePropertyProcessorSettingId SettingVersion.quotePropertyProcessorSettingVersion


    let tryUpsertQuotePropertyValidatorSetting =
        tryUpsertSetting<QuotePropertyProcessorSetting> SettingId.quotePropertyValidatorSettingId SettingVersion.quotePropertyValidatorSettingVersion


    let private loadQuotePropertyData (reader : DynamicSqlDataReader) =
        {
            QuotePropertyId = reader?QuotePropertyId
            StreetLine = reader?Street1
            City = reader?City
            State = reader?StateCode
            ZipCode = reader?Zip
        }


    let loadQuotePropertyTbl (CoreConnectionGetter getConn) startId endId =
        let g () =
            seq
                {
                    use conn = new DynamicSqlConnection(getConn)
                    use data = conn?<<quotePropertyDataSelectStr
                    data?startId <- startId
                    data?endId <- endId
                    use reader = data.ExecuteReader()
                    while reader.Read() do yield loadQuotePropertyData reader
                }
                |> List.ofSeq
                |> Ok

        tryQuotePropertyDbFun LoadQuotePropertyDataErr g


    /// Based on https://www.sqlservercentral.com/articles/the-output-clause-for-the-merge-statements
    let tryUpsertStreetZipAddOn (RatingConnectionGetter getConn) (q : StreetZipInfo) =
        let g() =
            use conn = new DynamicSqlConnection(getConn)

            let cmtText = @"
                declare @StreetFullNameVar nvarchar(100) = @StreetFullName
                declare @CityVar nvarchar(50) = @City
                declare @StateVar nvarchar(2) = @StateCode
                declare @ZipCodeVar nvarchar(100) = @Zip
                declare @StreetOriginalNameVar nvarchar(100) = @StreetOriginalName

                if ((select count(1) from EFStreetZips where StreetFullName = @StreetFullNameVar and City = @CityVar and StateCode = @StateVar and Zip = @ZipCodeVar) = 0) begin
                    merge EFStreetZipAddOns with (holdlock) as target
                    using (select @StreetFullNameVar, @CityVar, @StateVar, @ZipCodeVar, @StreetOriginalNameVar) as source (StreetFullName, City, StateCode, Zip, StreetOriginalName)
                    on (target.StreetFullName = source.StreetFullName and target.City = source.City and target.StateCode = source.StateCode and target.Zip = source.Zip)
                    when not matched then
                        insert (StreetFullName, City, StateCode, Zip, OccurrenceCount, StreetOriginalName)
                        values (source.StreetFullName, source.City, source.StateCode, source.Zip, 1, source.StreetOriginalName)
                    when matched then
                        update set StreetFullName = source.StreetFullName, City = source.City, StateCode = source.StateCode, Zip = source.Zip, StreetOriginalName = source.StreetOriginalName
                    output $action as MergeAction;
                end
                "

            use data = conn?<<cmtText
            data?StreetFullName <- q.key.StreetFullName
            data?City <- q.key.City
            data?StateCode <- q.key.State.key
            data?Zip <- q.key.ZipCode
            data?StreetOriginalName <- q.StreetOriginalName
            let result = data.ExecuteScalar() |> string

            match result with
            | EmptyString -> Ok None
            | _ ->
                match MergeAction.tryCreate result with
                | Some m -> Ok (Some m)
                | None -> $"q = %A{q}, m = %s{result}" |> UpsertStreetZipAddOnErr |> QuotePropertyErr |> Error

        tryQuotePropertyDbFun UpsertStreetZipAddOnExn g


    let getMaxQuotePropertyId (CoreConnectionGetter getConn) =
        let g() =
            seq
                {
                    use conn = new DynamicSqlConnection(getConn)
                    use data = conn?<<selectMaxQuotePropertyIdSql
                    use reader = data.ExecuteReader()
                    while reader.Read() do yield int reader?MaxQuotePropertyId
                }
            |> Seq.tryHead
            |> Option.defaultValue 0
            |> Ok

        tryQuotePropertyDbFun UnableToGetMaxQuotePropertyIdErr g
