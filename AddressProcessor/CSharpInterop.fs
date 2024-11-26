namespace Softellect.AddressProcessor

open Configuration
open AddressTypes
open Errors
open Swyfft.Common.SetDefinitions.CommonSets
open Swyfft.Common.Rop
open DataParsing

open Microsoft.Data.SqlClient

/// Collection of records made out of primitive types and helper functions to be used in C# <-> F# interop.
module CSharpInterop =

    /// Exposes F# unit to C# in case you ever need it there.
    let Unit = ()


    let toSwyfftResult<'T> (x : ApResult<'T>) =
        match x with
        | Ok v -> SwyfftResult<'T>.ToSwyfftResult(v)
        | Error e -> SwyfftResult<'T>.ToSwyfftResult(SwyfftFailure $"%A{e}")


    let toOptSwyfftResult<'T> (x : ApResult<'T option>) =
        match x with
        | Ok (Some v) -> SwyfftResult<'T>.ToSwyfftResult(v)
        | Ok None -> SwyfftResult<'T>.ToSwyfftResult(SwyfftFailure ("Unable to load data."))
        | Error e -> SwyfftResult<'T>.ToSwyfftResult(SwyfftFailure $"%A{e}")


    let toUnitSwyfftResult x =
        match x with
        | Ok() -> SwyfftResult.NotNeeded
        | Error e -> SwyfftResult.ToSwyfftResult(SwyfftFailure $"%A{e}")


    type AddressProcessorResultError =
        | ParsedSuccessfully = 0
        | CityNotFound = 1
        | DuplicateAddress = 2
        | HouseNumberNotFound = 3
        | InvalidAddress = 4
        | StateNotFound = 5
        | StreetNotFound = 6
        | TooLarge = 7
        | ZipNotFound = 8
        | CriticalError = 9
        | InvalidHouseNumber = 10
        | InvalidUnitNumber = 11
        | ExceptionOccurred = 1_000
        | ComparisonAddressKeyNotMatched = 1_001


    type SqlConnectionData =
        {
            getCoreConn : unit -> SqlConnection
            getRatingConn : unit -> SqlConnection
        }

        member this.coreConnectionGetter = CoreConnectionGetter this.getCoreConn
        member this.ratingConnectionGetter = RatingConnectionGetter this.getRatingConn


    type AddressProcessorInputParams =
        {
            RemoveUnitNumber : bool
            UseAbbreviatedNames : bool
            IgnoreStreetNotFound : bool
            DoSimpleParse : bool
            ExpandHyphen : bool
            DoFuzzySearch : bool
        }


    type AddressProcessorOutputParams =
        {
            GetAddressKey : bool
            GetAddressInferenceType : bool
        }


    /// Primitive record to be supplied from C#.
    type AddressProcessorParseParams =
        {
            InputParams : AddressProcessorInputParams
            OutputParams : AddressProcessorOutputParams
        }


    /// Primitive record to be supplied from C#.
    type AddressProcessorParseRequest =
        {
            AddressString : string
            ParseParams : AddressProcessorParseParams
        }


    /// Primitive record to be consumed in C#.
    type AddressProcessorResult =
        {
            Status : AddressProcessorResultError
            HasAddressKey : bool
            HasAddressInferenceType : bool
            MatchError : string
            Address : Address // Don't use in C# without extreme need.
            ParsedAddress : string
            AddressKey : string
            AddressInferenceType : AddrInferenceType
            FuzzySearchInfo : string
        }

        member r.IsParsedSuccessfully = if r.Status = AddressProcessorResultError.ParsedSuccessfully then true else false
        member r.IsStreetNotFound = if r.Status = AddressProcessorResultError.StreetNotFound then true else false


    type QuotePropertyProcessorParam =
        {
            sqlConnectionData : SqlConnectionData
            logInfo : string -> unit
            logError : string -> unit
        }


    type QuotePropertyProcessorRequest =
        {
            /// Set to true to start processing again and save default config into EFAddressProcessorSettings.
            resetProcessing : bool

            /// Set to non-negative value to process approximately that number of properties.
            /// Set to 0 to let QPP decide how many properties process in one go.
            /// Set to negative value to process all.
            numberToProcess : int

            /// Set to some non-empty string to use as an input file instead of EFQuoteProperties table.
            inputFile : string
        }


    type QuotePropertyProcessorResult =
        {
            /// If true, then processing was successfully reset.
            reset : bool

            /// If true, then QPP processed all properties.
            allProcessed : bool

            /// Total number of processed properties.
            processedProperties : int

            /// Total number of streets in processed properties.
            streets : int

            /// Total number of new streets in processed properties.
            newStreets : int

            /// Total number of errors occurred during processing.
            errors : int
        }

        static member defaultValue =
            {
                reset = false
                allProcessed = false
                processedProperties = 0
                streets = 0
                newStreets = 0
                errors = 0
            }


        static member allProcessedValue =
            {
                reset = false
                allProcessed = true
                processedProperties = 0
                streets = 0
                newStreets = 0
                errors = 0
            }


    /// All settings used by PropertyValidator.
    /// ! If the structure changes then increment value of SettingVersion.quotePropertyValidatorSettingVersion
    type QuotePropertyValidatorSetting =
        {
            quotePropertyId : int
        }

        static member defaultValue : QuotePropertyValidatorSetting =
            {
                quotePropertyId = 0
            }


    /// Loads QuoteProperty Validator settings.
    let loadQuotePropertyValidatorProcessorSetting getRatingConn =
        tryLoadSetting<QuotePropertyValidatorSetting> SettingId.quotePropertyValidatorSettingId SettingVersion.quotePropertyValidatorSettingVersion (RatingConnectionGetter getRatingConn)
        |> toOptSwyfftResult


    /// Upserts QuoteProperty Validator settings.
    let upsertQuotePropertyValidatorSetting (getRatingConn, q) =
        tryUpsertSetting<QuotePropertyValidatorSetting> SettingId.quotePropertyValidatorSettingId SettingVersion.quotePropertyValidatorSettingVersion (RatingConnectionGetter getRatingConn) q
        |> toUnitSwyfftResult


    /// Gets the max value of QuotePropertyId.
    let getMaxQuotePropertyId c = getMaxQuotePropertyId c |> toSwyfftResult
