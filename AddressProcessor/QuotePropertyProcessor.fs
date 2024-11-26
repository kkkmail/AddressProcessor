namespace Softellect.AddressProcessor

open CSharpInterop
open QuotePropertyProcessorRules
open Errors


//type QuotePropertyProcessor (p : QuotePropertyProcessorParam) =
//    let processPropertiesImpl r =
//        let data = getData p (Some r.inputFile)
//        let result = processQuoteProperties data r

//        match result with
//        | Ok (x, None) -> x |> Ok |> toSwyfftResult
//        | Ok (x, Some e) ->
//            let logError e = $"%A{e}" |> p.logError

//            match e with
//            | AggregateErr (a, b) -> (a :: b) |> List.map logError |> ignore
//            | _ -> logError e

//            x |> Ok |> toSwyfftResult
//        | Error e -> e |> Error |> toSwyfftResult


//    member _.processProperties (r : QuotePropertyProcessorRequest) : SwyfftResult<QuotePropertyProcessorResult> =
//        processPropertiesImpl r
