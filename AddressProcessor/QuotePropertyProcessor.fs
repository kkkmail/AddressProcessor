namespace Softellect.AddressProcessor

open CSharpInterop
open QuotePropertyProcessorRules
open Errors


type QuotePropertyProcessor (p : QuotePropertyProcessorParam) =
    let processPropertiesImpl r =
        let data = getData p (Some r.inputFile)
        let result = processQuoteProperties data r

        match result with
        | Ok (x, None) -> x |> Ok
        | Ok (x, Some e) ->
            let logError e = $"%A{e}" |> p.logError

            match e with
            | AggregateErr (a, b) -> (a :: b) |> List.map logError |> ignore
            | _ -> logError e

            x |> Ok
        | Error e -> e |> Error


    member _.processProperties (r : QuotePropertyProcessorRequest) = processPropertiesImpl r
