namespace Softellect.AddressProcessor

/// All errors known by the AP subsystem.
/// Based on:
/// https://fsharpforfunandprofit.com/posts/recipe-part2/
/// https://github.com/kkkmail/ClmFSharp/blob/kk-ContGen/Clm/ClmSys/Rop.fs (extension of the above)
/// https://github.com/kkkmail/ClmFSharp/blob/kk-ContGen/Clm/ClmSys/GeneralErrors.fs
/// https://github.com/kkkmail/ClmFSharp/blob/kk-ContGen/Clm/DbData/DatabaseTypes.fs
module Errors =

    type DbError =
        | DbException of exn
        | UpsertSettingErr of int


    type JsonError =
        | JsonExn of exn


    type InvalidVersionNumberError =
        {
            currentVersion : int
            databaseVersion : int
        }


    type SettingError =
        | SettingExn
        | InvalidVersionNumberErr of InvalidVersionNumberError
        | UpsertSettingExn


    type ProcessQuotePropertyDataError =
        | MissingDataErr of int
        | InvalidZipeCodeErr of int * string
        | InvalidCity of int * string
        | UnableToLoadMaxQuotePropertyIdErr
        | UnableToLoadQuotePropertySettingsErr


    type QuotePropertyError =
        | QuotePropertyGeneralFailureErr
        | LoadQuotePropertyDataErr
        | ProcessQuotePropertyDataErr of ProcessQuotePropertyDataError
        | UpsertStreetZipAddOnExn
        | UpsertStreetZipAddOnErr of string
        | UnableToGetMaxQuotePropertyIdErr
        | UnableToLoadDataErr of string


    type ApError =
        | AggregateErr of ApError * List<ApError>
        | DbErr of DbError
        | JsonErr of JsonError
        | SettingErr of SettingError
        | QuotePropertyErr of QuotePropertyError

        static member (+) (a, b) =
            match a, b with
            | AggregateErr (x, w), AggregateErr (y, z) -> AggregateErr (x, w @ (y :: z))
            | AggregateErr (x, w), _ -> AggregateErr (x, w @ [b])
            | _, AggregateErr (y, z) -> AggregateErr (a, y :: z)
            | _ -> AggregateErr (a, [b])

        member e.errorCount =
            match e with
            | AggregateErr (a, b) -> 1 + b.Length
            | _ -> 1


    type ApResult<'T> = Result<'T, ApError>
    type UnitResult = ApResult<unit>
    type ListResult<'T> = Result<list<Result<'T, ApError>>, ApError>


    /// ! Note that we cannot elevate to Result here as it will broaden the scope !
    /// Folds list<ApError> in a single ApError.
    let foldErrors (a : list<ApError>) =
        match a with
        | [] -> None
        | h :: t -> t |> List.fold (fun acc r -> r + acc) h |> Some


    /// Converts and error option into a unit result.
    let toUnitResult fo =
        match fo with
        | None -> Ok()
        | Some f -> Error f


    /// Folds list<ApError>, then converts to UnitResult.
    let foldToUnitResult = foldErrors >> toUnitResult


    let addError v (f : ApError) =
        match v with
        | Ok r -> Ok r
        | Error e -> Error (f + e)


    let combineUnitResults (r1 : UnitResult) (r2 : UnitResult) =
        match r1, r2 with
        | Ok(), Ok() -> Ok()
        | Error e1, Ok() -> Error e1
        | Ok(), Error e2 -> Error e2
        | Error e1, Error e2 -> Error (e1 + e2)


    let foldUnitResults (r : list<UnitResult>) =
        let rec fold acc w =
            match w with
            | [] -> acc
            | h :: t -> fold (combineUnitResults h acc) t

        fold (Ok()) r


    /// Splits the list of results into list of successes and list of failures.
    let unzipResults r =
        let success e =
            match e with
            | Ok v -> Some v
            | Error _ -> None

        let failure e =
            match e with
            | Ok _ -> None
            | Error e -> Some e

        let sf e = success e, failure e
        let s, f = r |> List.map sf |> List.unzip
        s |> List.choose id, f |> List.choose id


    let apOk() : UnitResult = Ok()


    let toErrorOpt r =
        match r with
        | Ok() -> None
        | Error e -> Some e
