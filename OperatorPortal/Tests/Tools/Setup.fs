module Tests.Setup

open System
open Testcontainers.PostgreSql
open Testcontainers.Azurite
open Xunit

type Setup() =
    do
        let postgres =
            PostgreSqlBuilder()
                .WithReuse(true)
                .WithName("foodbank_db")
                .WithEnvironment("POSTGRES_USER", "postgres")
                .WithEnvironment("POSTGRES_PASSWORD", "Strong!Passw0rd")
                .WithEnvironment("POSTGRES_DB", "food_bank")
                .WithPortBinding(5432, false)
                .Build()

        let azurite =
            AzuriteBuilder()
                .WithReuse(true)
                .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
                .WithName("azurite")
                .WithPortBinding(10000, 10000)
                .Build()

        postgres.StartAsync() |> Async.AwaitTask |> Async.RunSynchronously
        azurite.StartAsync() |> Async.AwaitTask |> Async.RunSynchronously

        let code = Migrations.main [||]
        code |> ignore

    interface IDisposable with
        member _.Dispose() = ()

[<assembly: CaptureConsole>]
do ()

[<assembly: AssemblyFixture(typeof<Setup>)>]
do ()
