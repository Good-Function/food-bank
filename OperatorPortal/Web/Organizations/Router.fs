module Organizations.Router

open System
open System.Security.Claims
open Microsoft.AspNetCore.Http
open Microsoft.FSharp.Reflection
open Organizations.Application
open Organizations.Application.ReadModels.FilterOperators
open Organizations.Application.ReadModels.OrganizationSummary
open Organizations.Application.ReadModels.OrganizationDetails
open Organizations.Application.ReadModels.QueriedColumn
open Organizations.List
open Organizations.Templates
open FsToolkit.ErrorHandling
open Organizations.Templates.List
open Oxpecker
open RenderBasedOnHtmx
open Organizations.CompositionRoot
open HttpContextExtensions
open type Microsoft.AspNetCore.Http.TypedResults

let tryParseInt (text: string) =
    match Int32.TryParse text with
    | true, value -> Some value
    | false, _ -> None
    
let emptyToNone (text: string) =
    if text = "" then None else Some text

let tryParseDU<'T> (input: string) : 'T option =
    if FSharpType.IsUnion typeof<'T> then
        FSharpType.GetUnionCases typeof<'T>
        |> Array.tryFind _.Name.Equals(input, StringComparison.OrdinalIgnoreCase)
        |> Option.map (fun case -> FSharpValue.MakeUnion(case, [||]) :?> 'T)
    else None

let parseFilter (ctx: HttpContext) : Query =
    let search = ctx.TryGetQueryValue "search" |> Option.defaultValue ""
    let sortBy = option {
        let! sortBy = ctx.TryGetQueryValue "sort" |> Option.bind tryParseDU<QueriedColumn>
        let! dir = ctx.TryGetQueryValue "dir"
        return sortBy, dir |> Direction.FromString
    }
    let page = ctx.TryGetQueryValue "page"
               |> Option.bind tryParseInt
               |> Option.defaultValue 1
    let filters =
        QueriedColumn.All |> List.map(fun column ->
            let operator = ctx.TryGetQueryValue $"{column}_op" |> Option.bind(FilterOperator.TryParse)
            let value: obj option =
                match operator with
                | Some (TextOperator _) -> ctx.TryGetQueryValue $"{column}"
                                           |> Option.bind emptyToNone
                                           |> Option.map box
                | Some (NumberOperator _) -> ctx.TryGetQueryValue $"{column}"
                                             |> Option.bind tryParseInt
                                             |> Option.map box
                | None -> None
            (value, operator) ||> Option.map2(fun value operator -> {Key = column; Value = value; Operator = operator})
        ) |> List.choose id
    {
        SearchTerm = search
        SortBy = sortBy
        Filters = filters
        Pagination = { Size = 50; Page = page; }
    }

let indexPage: EndpointHandler =
    fun ctx ->
        let username = ctx.User.FindFirstValue(ClaimTypes.Name)
        ctx.WriteHtmlView(SearchableListTemplate.FullPage Query.Zero username)
        
let summaries (readSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let username = ctx.User.FindFirstValue(ClaimTypes.Name)
            let filter = ctx |> parseFilter
            let! summaries, total = readSummaries filter
            return ctx |> render (ListTemplate.Template summaries filter total) (SearchableListTemplate.FullPage filter username)
        }
        
let details (readDetailsBy: ReadOrganizationDetailsBy) (id: int64) : EndpointHandler =
    fun ctx ->
        task {
            let username = ctx.User.FindFirstValue(ClaimTypes.Name)
            let! details = readDetailsBy id
            return
                ctx
                |> render (DetailsTemplate.Template details) (DetailsTemplate.FullPage details username)
        }
    
let import: EndpointHandler =
    fun ctx -> task {
        let username = ctx.User.FindFirstValue(ClaimTypes.Name)
        return ctx |> render (ImportExcelTemplate.Partial username None) (ImportExcelTemplate.FullPage username None)
    }
    
let upload (import: CreateOrganizationCommandHandler.Import): EndpointHandler =
    fun ctx -> task {
        match ctx.TryGetFirstFile with
        | Some file ->
            match! import (file.OpenReadStream()) with
            | Ok output -> return! ctx.WriteHtmlView (ImportExcelResultTemplate.Template output)
            | Error err ->
                ctx.SetStatusCode(StatusCodes.Status400BadRequest)
                return! ctx.WriteHtmlView (ImportExcelTemplate.Upload (Some $"%A{err}"))
        | None ->
            ctx.SetStatusCode(StatusCodes.Status400BadRequest)
            return! ctx.WriteHtmlView (ImportExcelTemplate.Upload (Some "Niepoprawny plik excel (.xlsx)"))
    }

let Endpoints (dependencies: Dependencies) =
    [ GET
          [ route "/" indexPage
            route "/summaries" (summaries dependencies.ReadOrganizationSummaries)
            routef "/{%d}" (details dependencies.ReadOrganizationDetailsBy)
            route "/import" import
          ]
      POST [
          route "/import/upload" (upload dependencies.Import)
      ]
      subRoute "/" (SectionsRouter.Endpoints dependencies)
    ]
