module Migrations

open System
open System.IO
open DbUp
open DbUp.Helpers
open DbUp.ScriptProviders

let logAndParseEngineResult (result: Engine.DatabaseUpgradeResult) =
    match result.Successful with
    | true ->
        Console.ForegroundColor <- ConsoleColor.Green
        Console.WriteLine "Finished: Success"
        Console.ResetColor()
        0
    | false ->
        Console.ForegroundColor <- ConsoleColor.Red
        Console.WriteLine result.Error
        Console.WriteLine "Finished: Failed"
        Console.ResetColor()
        -1

[<EntryPoint>]
let main argv =
    let connectionString =
        match argv |> Array.tryHead with
        | Some connectionString -> connectionString
        | None -> "Host=localhost;Port=5432;User Id=postgres;Password=Strong!Passw0rd;Database=food_bank;"
        
    let isExternalRunner =
        match argv |> Array.tryItem 1 with
        | Some "externalrunner" -> true
        | _ -> false

    let options =
        FileSystemScriptOptions(Filter = (fun sqlFilePath -> true), IncludeSubDirectories = true)

    let path =
        match isExternalRunner with
        | true -> AppDomain.CurrentDomain.BaseDirectory
        | false -> Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, @"../", "Web"))

    DeployChanges.To
        .PostgresqlDatabase(connectionString)
        .WithScriptsFromFileSystem(path, options)
        .JournalTo(NullJournal())
        .LogToConsole()
        .Build()
        .PerformUpgrade()
    |> logAndParseEngineResult
