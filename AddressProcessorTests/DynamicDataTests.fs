namespace Softellect.AddressProcessorTests

open System
open Xunit
open Xunit.Abstractions
open Softellect.AddressProcessor.DynamicData
open Softellect.AddressProcessor.AddressTypes
open Softellect.AddressProcessor
open Softellect.AddressProcessor.MatchingRules
open Softellect.AddressProcessor.Configuration
open System.Diagnostics
open FSharp.Collections.ParallelSeq

open Microsoft.EntityFrameworkCore
open Microsoft.Data.SqlClient

/// !!! Always explicitly specify types of parameters in each test with InlineData !!!
/// Otherwise test discovery may hang up if the underlying functions change signatures.
type DynamicDataTests(output : ITestOutputHelper) =

    let zipCode = ZipCode "71909"
    let streetKey = [ "ALDAYA"; "LN" ]
    let repetitions = 10000


    /// The purpose of this test is to measure performance of ZipMapUpdater
    /// under consecutive and parallel load.
    [<Fact>]
    member _.zipMapUpdaterTest() : unit =
        let conn = failwith "conn is not implemented."
        let getConn = RatingConnectionGetter (fun () -> new SqlConnection(conn))
        let updater = zipMapUpdater getConn
        updater.updateContent zipCode

        let tryGetValue _ = updater.getContent().TryFind zipCode |> Option.bind (fun e -> e |> Map.tryFind streetKey)
        let tryGetValueWithTryFind _ = (updater.getContent().TryFind zipCode |> toTryFind) |> Option.bind (fun e -> e streetKey)
        let data = [ for i in 1..repetitions -> i ]

        let timed f =
            let sw = Stopwatch()
            sw.Start()
            let _ = f data
            sw.Elapsed

        let runList () = timed (List.map tryGetValue)
        let runPSeq () = timed (PSeq.map tryGetValue >> PSeq.toList)
        //let runParallel () = timed (fun d -> ParallelHelpers.SelectParallel (d, fun _ -> tryGetValue(), Environment.ProcessorCount * 2))
        //let runParallelWithTryFind () = timed (fun d -> ParallelHelpers.SelectParallel (d, fun _ -> tryGetValueWithTryFind(), Environment.ProcessorCount * 2))

        //let listTime = runList()
        //let pSeqTime = runPSeq()
        //let parallelTime = runParallel ()
        //let parallelWithTryFindTime = runParallelWithTryFind ()

        //$"listTime = %A{listTime}\npSeqTime = %A{pSeqTime}\nparallelTime = %A{parallelTime}\nparallelWithTryFindTime = %A{parallelWithTryFindTime}\n" |> output.WriteLine

        ()
