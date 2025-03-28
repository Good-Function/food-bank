module Migrations

open System
open System.IO
open DbUp
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
    let isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") <> "Production"
    let connectionString =
        match argv |> Array.tryHead with
        | Some connectionString -> connectionString
        | None -> "Host=localhost;Port=5432;User Id=postgres;Password=Strong!Passw0rd;Database=food_bank;"
    let path =
        match argv |> Array.tryItem 1 with
        | Some path -> path
        | _ -> Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, @"../", "Web"))
    let options =
        FileSystemScriptOptions(Filter =
            (fun sqlFilePath -> isDev || not <| sqlFilePath.EndsWith("sample.sql")), IncludeSubDirectories = true)

    DeployChanges.To
        .PostgresqlDatabase(connectionString)
        .WithScriptsFromFileSystem(path, options)
        .LogToConsole()
        .WithVariable("FirstPassword", "$2a$11$/tUlXGocZ/FWGgs/SrKZlupPUiFi4PIu4LXhD5MfaO49PF9.1ZyfK")
        .Build()
        .PerformUpgrade()
    |> logAndParseEngineResult
