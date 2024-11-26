namespace Softellect.AddressProcessor

open System.IO
open System.IO.Compression
open CsvHelper
open System.Globalization
open UnionFactories

module DataUtilities =

    type private Marker =
        | Marker


    let openStreamFromDll (fileName : string) : Stream =
        let asm = typeof<Marker>.Assembly

        // Uncomment if GetManifestResourceStream starts returning null, then check the names and adjust as appropriate.
        //let resources = asm.GetManifestResourceNames()
        asm.GetManifestResourceStream(fileName)


    let openZippedStreamFromDll (zipFileName : string) (entryName : string) : Stream =
        let src = openStreamFromDll zipFileName
        let archive = new ZipArchive(src)
        let entry = archive.GetEntry(entryName)
        let strm = entry.Open()
        strm


    let openStream (fileName : string) : Stream = File.OpenRead(fileName) :> Stream
    let dataArchive (dataFile : string) : ZipArchive = new ZipArchive(openStream dataFile)


    let openStreamFromZip (zipFileName : string) (entryName : string) : Stream =
        (dataArchive zipFileName).GetEntry(entryName).Open()


    let streamToArrays (stream : Stream) : string[][] =
        use reader = new StreamReader(stream)
        let culture = CultureInfo.CurrentCulture
        use csv = new CsvParser(reader, culture)
        let result = ResizeArray()

        let rec readNextLine () =
            match csv.Read() with
            | false -> result.ToArray ()
            | true ->
                result.Add (csv.Record)
                readNextLine ()

        readNextLine()


    let readZipStates (s : Stream) =
        let allLines = streamToArrays s

        allLines.[1..]
        |> Array.map (fun line -> line.[0], line.[1])
        |> Array.choose (fun (z, s) -> match stateFactory.tryFromKey s with | Some t -> Some (z, t) | None -> None)
        |> Array.groupBy (fun (z, _) -> z)
        |> Array.map (fun (z, e) -> (z, e |> Array.map (fun (z, s) -> s) |> Set.ofArray))
        |> Map.ofArray


    let readCsvStreamAsMap (s : Stream) =
        let allLines = streamToArrays s
        let header = allLines.[0]
        allLines.[1..] |> Array.map (fun line -> Array.zip header line |> Map.ofArray)
