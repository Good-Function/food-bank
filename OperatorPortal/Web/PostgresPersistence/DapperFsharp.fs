module PostgresPersistence.DapperFsharp

open System.Data
open Dapper
open Npgsql
open OptionHandler

let dbSingleOrNone<'Result> (query: string) (param: obj) (connection: IDbConnection) : Async<'Result option> =
    async {
        let! result = connection.QuerySingleOrDefaultAsync<'Result>(query, param) |> Async.AwaitTask

        return
            match box result with
            | null -> None
            | _ -> Some result
    }

let dbSingle<'Result> (query: string) (param: obj) (connection: IDbConnection) : Async<'Result> =
    async {
        let! result = connection.QuerySingleAsync<'Result>(query, param) |> Async.AwaitTask
        return result
    }

let dbQuery<'Result> (query: string) (connection: IDbConnection) : Async<'Result seq> =
    connection.QueryAsync<'Result>(query) |> Async.AwaitTask

let dbQueryMultiple (query: string) (param: obj) (connection: IDbConnection) : Async<SqlMapper.GridReader> =
    connection.QueryMultipleAsync(query, param) |> Async.AwaitTask

let dbExecute (sql: string) (param: obj) (connection: IDbConnection) =
    connection.ExecuteAsync(sql, param) |> Async.AwaitTask |> Async.Ignore

let dbConnect (connectionString: string) : unit -> Async<IDbConnection> =
    OptionHandler.RegisterTypes()

    fun () ->
        async {
            let connection = new NpgsqlConnection(connectionString)

            if connection.State <> ConnectionState.Open then
                do! connection.OpenAsync() |> Async.AwaitTask

            return connection :> IDbConnection
        }
