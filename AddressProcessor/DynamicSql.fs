namespace Softellect.AddressProcessor

// https://github.com/dotnet/fsharp/issues/14011
#nowarn "1173"

open System
open System.Data
open Configuration

open Microsoft.Data.SqlClient

module DynamicSql =
    // --------------------------------------------------------------------------------------
    // Wrappers with dynamic operators for creating SQL Store Procedure calls
    // http://tomasp.net/blog/dynamic-sql.aspx/
    type DynamicSqlDataReader(reader:SqlDataReader) =
        member private _.Reader = reader
        member _.Read() = reader.Read()

        // http://www.fssnip.net/hh/title/Dynamic-operator-with-null-handling
        static member (?) (dr:DynamicSqlDataReader, name:string) : 'R =
            let typ = typeof<'R>
            if typ.IsGenericType && typ.GetGenericTypeDefinition() = typedefof<option<_>> then
                if dr.Reader.[name] = box DBNull.Value then
                    (box null) :?> 'R
                else typ.GetMethod("Some").Invoke(null, [| dr.Reader.[name] |]) :?> 'R
            else dr.Reader.[name] :?> 'R

        interface IDisposable with
            member _.Dispose() = reader.Dispose()


    type DynamicSqlCommand(cmd:SqlCommand) =
        member private _.Command = cmd

        static member (?<-) (cmd:DynamicSqlCommand, name:string, value) =
            cmd.Command.Parameters.Add(SqlParameter("@" + name, box value)) |> ignore

        member _.ExecuteNonQuery() = cmd.ExecuteNonQuery()
        member _.ExecuteReader() = new DynamicSqlDataReader(cmd.ExecuteReader())
        member _.ExecuteScalar() = cmd.ExecuteScalar()
        member _.Parameters = cmd.Parameters

        interface IDisposable with
            member _.Dispose() = cmd.Dispose()


    type DynamicSqlConnection(conn:SqlConnection) =
        do openConnIfClosed conn

        member private _.Connection = conn

        /// Stored Procedure
        static member (?) (conn:DynamicSqlConnection, name) =
            let command = new SqlCommand(name, conn.Connection)
            command.CommandType <- CommandType.StoredProcedure
            new DynamicSqlCommand(command)

        /// SQL statement
        static member (?<<) (conn:DynamicSqlConnection, commandText) =
            let command = new SqlCommand(commandText, conn.Connection)
            command.CommandType <- CommandType.Text
            new DynamicSqlCommand(command)

        member _.Open() = openConnIfClosed conn
        new (connStr:string) = new DynamicSqlConnection(connStr)
        new (getConn:unit -> SqlConnection) = new DynamicSqlConnection(getConn())

        interface IDisposable with
            member _.Dispose() = conn.Dispose()
