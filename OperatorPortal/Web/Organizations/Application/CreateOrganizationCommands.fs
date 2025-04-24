module Organizations.Application.CreateOrganizationCommands

open System.IO
open System.Transactions
open FsToolkit.ErrorHandling
open Organizations.Application.WriteModels

type RowParsingError = int * string list

type ImportSummary =
    { ImportedCount: int; TotalCount: int }

type ImportResult = ImportSummary * RowParsingError list

type ImportError =
    | InvalidFile of string
    | InvalidHeaders of
        {| ExpectedHeaders: string list
           ActualHeaders: string list |}

type SaveMany = Organization list -> Async<unit>
type ParseOrganizations = Stream -> Result<Organization list * RowParsingError list, ImportError>
type Import = Stream -> Async<Result<ImportResult, ImportError>>

let importOrganizations: ParseOrganizations -> SaveMany -> Stream -> Async<Result<ImportResult, ImportError>> =
    fun parse saveMany stream ->
        asyncResult {
            let! organizations, errors = stream |> parse
            use scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
            do! organizations |> saveMany
            let summary =
                { ImportedCount = organizations.Length
                  TotalCount = organizations.Length + errors.Length }
            scope.Complete()
            return summary, errors
        }