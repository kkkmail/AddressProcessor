namespace Softellect.AddressProcessor

open System.Diagnostics

/// http://www.fssnip.net/bb/title/Exception-Retry-Computation-Expression
/// Modified by kk: https://github.com/kkkmail/ClmFSharp/blob/master/Clm/ClmSys/Retry.fs
module Retry =
    open System.Threading
    open System


    type RetryParams =
        {
            maxRetries : int
            waitBetweenRetries : int
        }

    let defaultRetryParams =
        {
            maxRetries = 5
            waitBetweenRetries = 10_000
        }


    type RetryMonad<'a> = RetryParams -> 'a
    let rm<'a> (f : RetryParams -> 'a) : RetryMonad<'a> = f


    let internal retryFunc<'a> (f : RetryMonad<'a>) =
        rm (fun retryParams ->
            let rec execWithRetry f i e =
                match i with
                | n when n = retryParams.maxRetries -> raise e
                | _ ->
                    try
                        f retryParams
                    with
                        | e ->
                            Thread.Sleep(retryParams.waitBetweenRetries)
                            execWithRetry f (i + 1) e
            execWithRetry f 0 (Exception())
            )


    type RetryBuilder() =
        member this.Bind (p : RetryMonad<'a>, f : 'a -> RetryMonad<'b>) =
            rm (fun retryParams ->
                let value = retryFunc p retryParams
                f value retryParams
            )

        member this.Return (x : 'a) = fun defaultRetryParams -> x
        member this.Run(m : RetryMonad<'a>) = m
        member this.Delay(f : unit -> RetryMonad<'a>) = f ()


    /// Builds a retry workflow using computation expression syntax.
    let retry = RetryBuilder()


    let tryFun logger f =
        try
            let p = Process.GetCurrentProcess()
            let c = p.PriorityClass

            try
                try
                    p.PriorityClass <- ProcessPriorityClass.High

                    (retry {
                        let! b = rm (fun _ -> f())
                        return Some b
                    }) defaultRetryParams
                with
                    | e ->
                        logger e
                        None
            finally
                p.PriorityClass <- c
        with
            | e ->
                logger e
                None


    let tryDbFun logger connectionString f = tryFun logger (fun () -> f connectionString)
