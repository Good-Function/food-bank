module Tests.Setup

open System
open System.Diagnostics
open System.IO
open Testcontainers.PostgreSql
open Xunit

let runFSharpScript scriptPath =
    let psi = ProcessStartInfo("dotnet", $"fsi {scriptPath}")
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.UseShellExecute <- false
    use proc = Process.Start(psi)
    proc.WaitForExit()

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
        let migrator = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, @"../../", "migrations.fsx"))
        runFSharpScript migrator
    
    interface IDisposable with
        member _.Dispose() = ()
[<assembly: CaptureConsole >] do ()
[<assembly: AssemblyFixture(typeof<Setup>)>] do ()


