namespace Softellect.AddressProcessor

open Swyfft.Services.AddressProcessor
open CSharpInterop

open Microsoft.Data.SqlClient

module ProcessAddresses =

    let input =
        [|
            (1, "1213 2nd Ave N Clanton, AL 35045")
        |]


    [<Literal>]
    let CoreDbConnectionString = "Server=localhost;Database=SwyfftCore;Integrated Security=SSPI"


    [<Literal>]
    let RatingDbConnectionString = "Server=localhost;Database=SwyfftRating;Integrated Security=SSPI"


    let data =
        {
            getCoreConn = fun () -> new SqlConnection(CoreDbConnectionString)
            getRatingConn = fun () -> new SqlConnection(RatingDbConnectionString)
        }

    let parser = AddressProcessor (data)


    //let processAddress i e =
    //    if i % 1000 = 0 then printfn "Parsing i = %A, e = %A" i e
    //    e |> parser.parse2


    //let processed =
    //    input
    //    |> Array.mapi (fun i (n, e) ->
    //            let (m, a, k) = processAddress i (e, false)
    //            (n, m, a, k, e))
    //    //|> Seq.toArray



    //let hasFailed (e : (string * (MatchError option * 'A option))) =
    //    let i, (m, r) = e
    //    match r with
    //    | Some _ -> None
    //    | _ ->
    //        //printfn "hasFailed::i = %A" i
    //        Some e


    ////let missed = processed |> Array.choose (fun e -> hasFailed e)

    //type CommercialAddressProcessor() =
    //    let runImpl() =
    //        printfn "input.Length = %A" input.Length

    //        let header = "ID,Error,Parsed,AddressKey,Original\n"

    //        let data =
    //            processed
    //            |> Array.map (fun (n, m, a, k, e) ->
    //                let n1 = n.ToString()
    //                let m1 =
    //                    match m with
    //                    | Some e -> e.ToString()
    //                    | None -> ""

    //                let a1 =
    //                    match a with
    //                    | Some e -> "\"" + (e.asString false) + "\""
    //                    | None -> ""

    //                let k1 =
    //                    match k with
    //                    | Some e -> "\"" + e.ToString() + "\""
    //                    | None -> ""

    //                sprintf "%s,%s,%s,%s,\"%s\"" n1 m1 a1 k1 e
    //                )
    //                |> String.concat("\n")

    //        File.WriteAllText(@"C:\Temp\core_il_addresses_results.csv", header + data)

    //    member __.Run() = runImpl()
