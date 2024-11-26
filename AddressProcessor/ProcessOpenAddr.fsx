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


open Swyfft.Services.AddressProcessor.OpenAddressesProcessing
open System.Data.SqlClient


let config =
    {
        getInputConn = (fun () -> new SqlConnection "Server=localhost;Database=SwyfftRating;Integrated Security=SSPI")
        getOutputConn = (fun () -> new SqlConnection "Server=localhost;Database=SwyfftRating;Integrated Security=SSPI")
        logError = printfn "%A"
        logInfo = printfn "%A"
        parallelRun = false
        batchSize = 100_000
    }

let oap = OpenAddressProcessor config

#time
oap.processAll ()
#time

printfn "All completed!"
