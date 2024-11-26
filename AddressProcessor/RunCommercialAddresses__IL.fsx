#r @"..\packages\FSharp.Core.4.7.0\lib\net45\FSharp.Core.dll"

#I __SOURCE_DIRECTORY__
#r "System.dll"
#r "System.Core.dll"
#r "System.Configuration.dll"
#r "System.Numerics.dll"
#r "System.IO"
#r "System.IO.Compression"
#r "System.Data"


open System
printfn "Starting at %A" DateTime.Now

let printSep () = printfn "\n*********************\n"

#r @"..\packages\System.ValueTuple.4.5.0\lib\net47\System.ValueTuple.dll"
#r @"..\packages\FSharp.Collections.ParallelSeq.1.1.2\lib\net45\FSharp.Collections.ParallelSeq.dll"
#r @"..\packages\FSharp.Data.SqlClient.2.0.5\lib\net40\FSharp.Data.SqlClient.dll"

#r @".\bin\Debug\Swyfft.Services.AddressProcessor.dll"

open System.Data.SqlClient
open FSharp.Collections.ParallelSeq
open Swyfft.Services.AddressProcessor
open Swyfft.Services.AddressProcessor.MatchTypes


let input =
    [|
        (1, "1213 2nd Ave N Clanton, AL 35045")
    |]

printfn "input.Length = %A" input.Length

[<Literal>]
let RatingDbConnectionString = "Server=localhost;Database=SwyfftRating;Integrated Security=SSPI"

let getconnection() = new SqlConnection(RatingDbConnectionString)


printfn "Initializing..."
#time
let parser = AddressProcessor getconnection
#time
printfn "... done."


let processAddress i e = 
    if i % 1000 = 0 then printfn "Parsing i = %A, e = %A" i e
    e |> parser.parse2


printfn "Parsing..."
#time
let processed =
    input
    |> PSeq.mapi (fun i (n, e) ->
            let (m, a, k) = processAddress i (e, false)
            (n, m, a, k, e))
    |> Seq.toArray

#time
printfn "... done."
printSep ()


let hasFailed (e : (string * (MatchError option * 'A option))) = 
    let i, (m, r) = e
    match r with 
    | Some _ -> None
    | _ -> 
        //printfn "hasFailed::i = %A" i
        Some e


//let missed = processed |> Array.choose (fun e -> hasFailed e)
printSep ()


printfn "ID,Error,Parsed,AddressKey,Original"
processed
|> Array.map (fun (n, m, a, k, e) ->
    let n1 = n.ToString()
    let m1 =
        match m with
        | Some e -> e.ToString()
        | None -> ""

    let a1 =
        match a with
        | Some e -> e.asString false
        | None -> ""

    let k1 =
        match k with
        | Some e -> e.ToString()
        | None -> ""

    printfn "%s,%s,%s,%s,%s" n1 m1 a1 k1 e
    )
printSep ()


//printfn "input.Lengh = %A" input.Length
//printfn "missed.Lengh = %A" missed.Length
//printfn "success rate = %A" ((float (input.Length -  missed.Length)) / (float (input.Length)))
//printSep ()
