namespace Softellect.AddressProcessor

open System.Data

open Microsoft.Data.SqlClient

module Configuration =


    /// Maximum number of words allowed in a single address group.
    /// Anything beyond that will be discarded.
    [<Literal>]
    let MaxNumberOfWords = 2000


    /// Address processor version.
    /// Increment every time when address processor code is updated.
    [<Literal>]
    let AddressProcessorVersion = 4000


    let openConnIfClosed (conn : SqlConnection) =
        match conn.State with
        | ConnectionState.Closed -> do conn.Open()
        | _ -> ignore ()


    let toOpenConn (conn : SqlConnection) =
        openConnIfClosed conn
        conn


    [<Literal>]
    let ZipStateName = @"ZipState.csv"


    [<Literal>]
    let ZipStateZipFileName = "Softellect.AddressProcessor.ZipState.zip"


    let doLog (logger : (string -> unit)) (message : string) : unit =
        do logger message


    type CoreConnectionGetter =
        | CoreConnectionGetter of (unit -> SqlConnection)

        member this.getConnection() = let (CoreConnectionGetter v) = this in v() |> toOpenConn


    type RatingConnectionGetter =
        | RatingConnectionGetter of (unit -> SqlConnection)

        member this.getConnection() = let (RatingConnectionGetter v) = this in v() |> toOpenConn


    /// Don't use without extreme need, e.g. when you can have a connection pointing to different databases.
    /// TODO kk:20200127 - It should be refactored properly to account for connection getters, which can point to different databases.
    type ConnectionGetter =
        | ConnectionGetter of (unit -> SqlConnection)

        member this.getConnection() = let (ConnectionGetter v) = this in v() |> toOpenConn


    /// ! Do not change the values!
    type SettingId =
        | SettingId of int

        member this.value = let (SettingId v) = this in v
        static member quotePropertyProcessorSettingId = SettingId 1
        static member quotePropertyValidatorSettingId = SettingId 2


    /// ! Increment every time when the structure of relevant setting JSON changes !
    type SettingVersion =
        | SettingVersion of int

        static member quotePropertyProcessorSettingVersion = SettingVersion 1
        static member quotePropertyValidatorSettingVersion = SettingVersion 1


    /// All settings used by QuotePropertyProcessor.
    type QuotePropertyProcessorSetting =
        {
            quotePropertyId : int
        }
