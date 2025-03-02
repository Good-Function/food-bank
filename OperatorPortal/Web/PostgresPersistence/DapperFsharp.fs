module PostgresPersistence.DapperFsharp

open System.Data
open Dapper
open Npgsql
open OptionHandler

type IDbConnection with

    member this.trySingle<'Result> (query: string) (param: obj) : Async<'Result option> =
        async {
            let! result = this.QuerySingleOrDefaultAsync<'Result>(query, param) |> Async.AwaitTask

            return
                match box result with
                | null -> None
                | _ -> Some result
        }

    member this.Single<'Result> (query: string) (param: obj) : Async<'Result> =
        async {
            let! result = this.QuerySingleAsync<'Result>(query, param) |> Async.AwaitTask
            return result
        }

    member this.Query<'Result>(query: string) =
        async {
            let! result = this.QueryAsync<'Result>(query) |> Async.AwaitTask
            return Seq.toList result
        }

    member this.QueryBy<'Result>(query: string) (param: obj) =
        async {
            let! result = this.QueryAsync<'Result>(query, param) |> Async.AwaitTask
            return Seq.toList result
        }

    member this.Execute (sql: string) (param: obj) =
        this.ExecuteAsync(sql, param) |> Async.AwaitTask |> Async.Ignore

let connectDB (connectionString: string) : unit -> Async<IDbConnection> =
    OptionHandler.RegisterTypes()

    fun () ->
        async {
            let connection = new NpgsqlConnection(connectionString)

            if connection.State <> ConnectionState.Open then
                do! connection.OpenAsync() |> Async.AwaitTask

            return connection :> IDbConnection
        }
