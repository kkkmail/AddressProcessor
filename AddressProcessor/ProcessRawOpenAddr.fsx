printfn "Starting..."
#I __SOURCE_DIRECTORY__
#r "System.dll"
#r "System.Core.dll"
#r "System.Data.dll"
#r "System.Configuration.dll"
#r "System.Numerics.dll"
#r "System.IO"
#r "System.IO.Compression"

#r @".\bin\Debug\System.Threading.Tasks.Extensions.dll"
#r @".\bin\Debug\Swyfft.Services.AddressProcessor.dll"


open Swyfft.Services.AddressProcessor.RawOpenAddressProcessing
open System.Data.SqlClient

let dir = @"C:\Source\oa\"


let srcArray =
    [|
        @"openaddr-collected-us_midwest.zip"
        @"openaddr-collected-us_northeast.zip"
        @"openaddr-collected-us_south.zip"
        @"openaddr-collected-us_west.zip"
    |]


let config =
    {
        getOutputConn = (fun () -> new SqlConnection "Server=localhost;Database=SwyfftRating;Integrated Security=SSPI")
        logError = printfn "%A"
        logInfo = printfn "%A"
        batchSize = 100_000
    }

let roap = RawOpenAddressProcessor config
roap.truncate()


#time
srcArray |> Array.map(fun s -> roap.processArchive (dir + s))
#time

printfn "All completed!"
