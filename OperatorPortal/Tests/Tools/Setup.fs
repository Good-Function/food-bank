module Tests.Setup

open System
open Testcontainers.PostgreSql
open Xunit

type Setup() =
    do
        let container = PostgreSqlBuilder()
                            .WithReuse(true)
                            .WithName("foodbank_db")
                            .WithEnvironment("POSTGRES_USER", "postgres")
                            .WithEnvironment("POSTGRES_PASSWORD", "Strong!Passw0rd")
                            .WithEnvironment("POSTGRES_DB", "food_bank")
                            .WithPortBinding(5432, false)
                            .Build();
        container.StartAsync() |> Async.AwaitTask |> Async.RunSynchronously
        Migrations.main([||]) |> ignore
    
    interface IDisposable with
        member _.Dispose() = ()
[<assembly: CaptureConsole >] do ()
[<assembly: AssemblyFixture(typeof<Setup>)>] do ()


