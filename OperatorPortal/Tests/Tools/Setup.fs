module Tests.Setup

open System
open System.Diagnostics
open System.Threading
open DotNet.Testcontainers.Builders
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
    if not (String.IsNullOrWhiteSpace(errors)) then
        printfn "Script Errors: %s" errors

type Setup() =
    do
        let container =
            ContainerBuilder()
                .WithImage("postgres:latest")
                .WithReuse(true)
                .WithName("foodbank_db")
                .WithEnvironment("POSTGRES_USER", "postgres")
                .WithEnvironment("POSTGRES_PASSWORD", "Strong!Passw0rd")
                .WithEnvironment("POSTGRES_DB", "food_bank")
                .WithPortBinding(5432, false)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build()
        container.StartAsync() |> Async.AwaitTask |> Async.RunSynchronously
        Thread.Sleep(1000)
        runFSharpScript "/home/marcin/code/food-bank/OperatorPortal/migrations.fsx"
    
    interface IDisposable with
        member _.Dispose() = ()
[<assembly: CaptureConsole >] do ()
[<assembly: AssemblyFixture(typeof<Setup>)>] do ()


