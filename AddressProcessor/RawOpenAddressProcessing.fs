namespace Softellect.AddressProcessor

open System
open System.IO
open DataUtilities
open OpenAddressDataTypes
open StringParser
open DynamicSql

open Microsoft.Data.SqlClient

module RawOpenAddressProcessing =

    [<Literal>]
    let CityPrefix = "city_of_"


    [<Literal>]
    let TownPrefix = "town_of_"


    [<Literal>]
    let CountySuffix = "_county"


    [<Literal>]
    let RawOpenAddressTblName = "EFRawOpenAddresses"


    let expectedHeader =
        [|
            "LON"
            "LAT"
            "NUMBER"
            "STREET"
            "UNIT"
            "CITY"
            "DISTRICT"
            "REGION"
            "POSTCODE"
            "ID"
            "HASH"
        |]


    let getFileName (s : string) = Path.GetFileNameWithoutExtension(s)
    let getFileExtension (s : string) = Path.GetExtension(s)
    let getCountryState (sa : string[]) : CountryState = { country = sa.[0].ToUpper(); state = sa.[1].ToUpper() }

    let fixCity (city : string) : string =
        city.Replace("_", " ").ToUpper()

    let getUnprefixed (s : string) (prefix : string) : string =
        if s.StartsWith(prefix)
        then (s.Substring(prefix.Length, s.Length - prefix.Length))
        else failwith $"Cannot match prefix = %A{prefix} with string = %A{s}"


    let getUnsuffixed (s : string) (suffix : string) : string =
        if s.EndsWith(suffix)
        then (s.Substring(0, s.Length - suffix.Length))
        else failwith $"Cannot match suffix = %A{suffix} with string = %A{s}"


    let isSource (sa : string[]) : bool =
        match sa.[0].ToLower() with
        | "summary" -> false
        | _ ->
            let fileNameExt = sa.[sa.Length - 1]
            match (getFileExtension fileNameExt).ToLower() with
            | ".csv" -> true
            | _ -> false


    let getSourceType (sa : string[]) : SourceType =
        let fileNameExt = sa.[sa.Length - 1]
        match (getFileName fileNameExt).ToLower() with
        | "statewide" -> getCountryState sa |> SourceType.StateWide
        | s ->
            let countryState = getCountryState sa

            if s.StartsWith(CityPrefix) then SourceType.City (countryState, (getUnprefixed s CityPrefix) |> fixCity)
            elif s.StartsWith(TownPrefix) then SourceType.Town (countryState, (getUnprefixed s TownPrefix) |> fixCity)
            elif s.EndsWith(CountySuffix) then SourceType.County (countryState, (getUnsuffixed s CountySuffix) |> fixCity)
            else SourceType.Unknown (countryState, s |> fixCity)


    let toLatLon (s : string) : decimal option =
        match Decimal.TryParse s with
        | true, v -> Some v
        | false, _ -> None

    // 0 - LON
    // 1 - LAT
    // 2 - NUMBER
    // 3 - STREET
    // 4 - UNIT
    // 5 - CITY
    // 6 - DISTRICT
    // 7 - REGION
    // 8 - POSTCODE
    // 9 - ID
    // 10 - HASH
    let toRowData (fullName : string) (sourceType : SourceType) (rowData : string[]) : RowData =
        {
            latitude = toLatLon rowData.[1]
            longitude = toLatLon rowData.[0]

            number = rowData.[2]
            street = rowData.[3]
            unit = rowData.[4]

            city =
                match rowData.[5] with
                | EmptyString ->
                    match sourceType with
                    | SourceType.City (_, s) -> s
                    | SourceType.Town (_, s) -> s
                    | SourceType.Unknown (_, s) -> s
                    | SourceType.StateWide _ -> EmptyString
                    | SourceType.County _ -> EmptyString
                | s -> s

            district =
                match rowData.[6] with
                | EmptyString ->
                    match sourceType with
                    | SourceType.County (_, s) -> s
                    | _ -> EmptyString // No county information from file name
                | s -> s

            region =
                match sourceType with
                | StateWide cs -> cs.state
                | City (cs, _) -> cs.state
                | Town (cs, _) -> cs.state
                | County (cs, _) -> cs.state
                | Unknown (cs, _) -> cs.state
            postCode =
                let z = rowData.[8]
                z.Substring (0, min 5 z.Length)

            id = rowData.[9]
            hash = rowData.[10]
            source = fullName
        }


    let readCsvStream (fullName : string) (sourceType : SourceType) (s : Stream) : RowData[] =
        let allLines = streamToArrays s
        let header = allLines.[0]

        if header = expectedHeader
        then
            allLines.[1..]
            |> Array.map (fun line ->
                            line
                            |> Array.map (fun l -> l.ToUpper())
                            |> toRowData fullName sourceType
                         )
        else
            failwith ("Incorrect header: ")


    type RawOpenAddressConfig =
        {
            getOutputConn : unit -> SqlConnection
            logError : (string -> unit)
            logInfo : (string -> unit)
            batchSize : int
        }

    type RawOpenAddressProcessor (config : RawOpenAddressConfig) =
        let getOutputConn = config.getOutputConn
        let logError = config.logError
        let logInfo = config.logInfo
        let batchSize = config.batchSize


        let truncateImpl () =
            use conn = new DynamicSqlConnection(getOutputConn)
            use data = conn?<<truncateOpenAddressSql
            data.ExecuteNonQuery() |> ignore
            use data1 = conn?<<truncateRawOpenAddressSql
            data1.ExecuteNonQuery() |> ignore


        let updateAddress (rowData : RowData) =
            try
                match rowData.latitude, rowData.longitude with
                | Some lat, Some lon when lat <> 0M && lon <> 0M ->
                    let cmdText = "
                        insert into dbo.EFRawOpenAddresses
                                (Latitude
                                ,Longitude
                                ,Number
                                ,Street
                                ,Unit
                                ,City
                                ,District
                                ,Region
                                ,Id
                                ,Hash
                                ,Source
                                ,PostCode)
                             values
                                (@Latitude
                                ,@Longitude
                                ,@Number
                                ,@Street
                                ,@Unit
                                ,@City
                                ,@District
                                ,@Region
                                ,@Id
                                ,@Hash
                                ,@Source
                                ,@PostCode)"

                    use conn = new DynamicSqlConnection(getOutputConn)
                    use data = conn?<<cmdText
                    data?Latitude <- lat
                    data?Longitude <- lon
                    data?Number <- rowData.number
                    data?Street <- rowData.street
                    data?Unit <- rowData.unit
                    data?City <- rowData.city
                    data?District <- rowData.district
                    data?Region <- rowData.region
                    data?Id <- rowData.id
                    data?Hash <- rowData.hash
                    data?Source <- rowData.source
                    data?PostCode <- rowData.postCode

                    data.ExecuteNonQuery() |> ignore
                | _ -> ignore ()
            with
            | ex ->
                logError "    !!! updateAddress::EXCEPTION !!!"
                logError $"    rowData = %A{rowData}"
                logError $"    ex = %A{ex}"
                failwith (ex.ToString())


        let processBatch (rowData : RowData[]) (startIndex : int) (endIndex : int) =
            for i in startIndex..endIndex do updateAddress rowData.[i]


        let processFile (fullName : string) (sourceType : SourceType) (s : Stream) =
            let data = readCsvStream fullName sourceType s
            let dataLen = data.Length
            logInfo $"        Number of rows to process: %A{dataLen}"

            let noOfBatches =
                let baseLen = dataLen / batchSize
                if baseLen * batchSize = dataLen then baseLen
                else baseLen + 1

            try
                for i in 0..noOfBatches - 1 do
                    logInfo $"        Processing batch # %A{i}"
                    let startIndex = i * batchSize
                    let endIndex = min ((i + 1) * batchSize - 1) (dataLen - 1)
                    processBatch data startIndex endIndex
            with
            | ex ->
                logInfo "    !!! processFile::EXCEPTION !!!"
                failwith (ex.ToString())


        let processArchiveImpl (src : string) =
            let za = dataArchive src
            let startTime = DateTime.Now

            logInfo $"Processing archive: %A{src}\n    Started at: %A{startTime}"

            let allEntries =
                za.Entries
                |> Seq.toArray

            let neededEntries =
                allEntries
                |> Array.map (fun e -> (e.FullName.Split([|'/'|], StringSplitOptions.RemoveEmptyEntries), e))
                |> Array.filter (fun (n, _) -> isSource n)
                |> Array.map (fun (n, e) -> getSourceType n, e)
                //|> Array.take 1

            logInfo "    Part #1 completed."
            logInfo $"    Number of files to process: %A{neededEntries.Length}, all files: %A{allEntries.Length}"

            neededEntries
            |> Array.mapi(fun i (n, e) ->
                    logInfo $"    i = %A{i}, e = %A{n}, full = %A{e.FullName}"
                    e.Open() |> processFile e.FullName n
                    )
            |> ignore

            let endTime = DateTime.Now

            logInfo $"    Part #2 completed. Run time: %A{endTime - startTime}"

        member _.processArchive = processArchiveImpl
        member _.truncate = truncateImpl
