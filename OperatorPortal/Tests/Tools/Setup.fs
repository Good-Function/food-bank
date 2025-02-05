module Tests.Setup

open System
open System.Collections.Concurrent
open System.Diagnostics
open System.IO
open System.Threading
open System.Threading.Tasks
open Testcontainers.PostgreSql
open Xunit

let run scriptPath  =
    let cts = TaskCompletionSource()
    use cmd = new Process()
    let cmdInfo = ProcessStartInfo()
    cmdInfo.FileName <- "bash"
    cmdInfo.RedirectStandardInput <- true
    cmdInfo.RedirectStandardOutput <- true
    cmdInfo.RedirectStandardError <- true
    cmdInfo.UseShellExecute <- false
    cmdInfo.WorkingDirectory <- scriptPath
    cmd.StartInfo <- cmdInfo
    let output = new BlockingCollection<string>()
    let append = fun (args: DataReceivedEventArgs) ->
        output.Add args.Data
        if args.Data = "_finito_" then cts.SetResult()
    cmd.ErrorDataReceived.Add append
    cmd.OutputDataReceived.Add append
    cmd.Start() |> ignore
    cmd.BeginOutputReadLine();
    cmd.BeginErrorReadLine();
    cmd.StandardInput.WriteLine($"dotnet fsi migrations.fsx ; echo _finito_")
    cts.Task.Wait()
    output |> Seq.toList |> List.filter(fun output -> output <> "_finito_")

let runFSharpScript scriptPath =
    let psi = ProcessStartInfo("dotnet", $"fsi {scriptPath}")
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.UseShellExecute <- false
    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "migrations_pre.txt"), "WILL RUN MIGRATIONS")
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
        run "/home/marcin/code/food-bank/OperatorPortal"
        ()
    
    interface IDisposable with
        member _.Dispose() = ()
[<assembly: CaptureConsole >] do ()
[<assembly: AssemblyFixture(typeof<Setup>)>] do ()


