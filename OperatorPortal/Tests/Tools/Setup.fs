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
    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "migrations_pre.txt"), "WILL RUN MIGRATIONS")
    use proc = Process.Start(psi)
    File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "migrations_pre.txt"), proc.ToString())
    File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "migrations_pre.txt"), proc.ProcessName)

    proc.WaitForExit()
    let output = proc.StandardOutput.ReadToEnd()
    let errors = proc.StandardError.ReadToEnd()
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
        let migrator = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, @"../../", "migrations.fsx"))
        runFSharpScript migrator
    
    interface IDisposable with
        member _.Dispose() = ()
[<assembly: CaptureConsole >] do ()
[<assembly: AssemblyFixture(typeof<Setup>)>] do ()


