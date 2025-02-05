module Tests.Setup

open System
open System.Diagnostics
open System.IO
open System.Threading
open Testcontainers.PostgreSql
open Xunit

let runFSharpScript scriptPath =
    let psi = ProcessStartInfo("dotnet", $"fsi {scriptPath}")
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.UseShellExecute <- false

    use proc = Process.Start(psi)
    let output = proc.StandardOutput.ReadToEnd()
    let errors = proc.StandardError.ReadToEnd()
    proc.WaitForExit()
    printfn "Script Output: %s" output
    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "migrations_out.txt"), output)
    if not (String.IsNullOrWhiteSpace(errors)) then
        printfn "Script Errors: %s" errors
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "migrations_err.txt"), output)

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
        Thread.Sleep(1000)
        runFSharpScript "/home/marcin/code/food-bank/OperatorPortal/migrations.fsx"
    
    interface IDisposable with
        member _.Dispose() = ()
[<assembly: CaptureConsole >] do ()
[<assembly: AssemblyFixture(typeof<Setup>)>] do ()


