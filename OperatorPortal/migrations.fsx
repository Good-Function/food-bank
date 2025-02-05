#r "nuget: DbUp"
#r "nuget: dbup-postgresql"

open System
open System.IO
open DbUp
open DbUp.Helpers

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
         
let connectionString =
  match fsi.CommandLineArgs |> Array.tryItem 1 with
  | Some connectionString -> connectionString
  | None ->
      "Host=localhost;Port=5432;User Id=postgres;Password=Strong!Passw0rd;Database=food_bank;"

let path = Path.Combine(__SOURCE_DIRECTORY__, "Web", "Organizations", "Database")

DeployChanges.To.PostgresqlDatabase(connectionString)
             .WithScriptsFromFileSystem(path, fun sqlFilePath -> true)
             .JournalTo(NullJournal())
             .LogToConsole().Build().PerformUpgrade()
|> logAndParseEngineResult