module Organizations.Router

open System
open Microsoft.AspNetCore.Http
open Microsoft.FSharp.Reflection
open Organizations.Application
open Organizations.Application.ReadModels.Filter
open Organizations.Application.ReadModels.FilterOperators
open Organizations.Application.ReadModels.OrganizationSummary
open Organizations.Application.ReadModels.OrganizationDetails
open Organizations.Application.ReadModels.QueriedColumn
open Organizations.Application.ReadModels.MailingList
open Organizations.List
open Organizations.Templates
open FsToolkit.ErrorHandling
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

let parseFilters (ctx: HttpContext): Filter list =
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

let parseQueryParams (ctx: HttpContext) : Query =
    let search = ctx.TryGetQueryValue "search" |> Option.defaultValue ""
    let sortBy = option {
        let! sortBy = ctx.TryGetQueryValue "sort" |> Option.bind tryParseDU<QueriedColumn>
        let! dir = ctx.TryGetQueryValue "dir"
        return sortBy, dir |> Direction.FromString
    }
    let page = ctx.TryGetQueryValue "page"
               |> Option.bind tryParseInt
               |> Option.defaultValue 1
    let filters = parseFilters ctx
    {
        SearchTerm = search
        SortBy = sortBy
        Filters = filters
        Pagination = { Size = 50; Page = page; }
    }

let indexPage: EndpointHandler =
    fun ctx ->
        let username = ctx.UserName
        ctx.WriteHtmlView(SearchableListTemplate.FullPage Query.Zero username)
        
let summaries (readSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let username = ctx.UserName
            let filter = ctx |> parseQueryParams
            let! summaries, total = readSummaries filter
            return ctx |> render (ListTemplate.Template summaries filter total) (SearchableListTemplate.FullPage filter username)
        }
        
let mailingList (readMailingList: ReadMailingList): EndpointHandler =
    fun ctx ->
        task {
            let filters = ctx |> parseFilters
            let search = ctx.TryGetQueryValue "search" |> Option.defaultValue ""
            let! mailingList = readMailingList(search, filters)
            let validMails, invalidMails = mailingList |> List.partition (fun mail -> mail.Email.Length > 3)
            let total = List.length mailingList
            let emailsCount = List.length validMails
            ctx.SetHttpHeader("HX-Trigger-After-Swap", $"""{{ "emailCopyDone": {{ "total": {total}, "count": {emailsCount} }} }}""" )
            return ctx.WriteHtmlView (MailingList.View {
                                                            Mails = validMails |> List.map _.Email |> String.concat ";"
                                                            MissingTeczka = invalidMails |> List.map _.Teczka.ToString() |> String.concat ", "
                                                        })
        }
        
let details (readDetailsBy: ReadOrganizationDetailsBy) (id: int64) : EndpointHandler =
    fun ctx ->
        task {
            let permissions = Permissions.rolePermissions[ctx.UserRole]
            let username = ctx.UserName
            let! details = readDetailsBy id
            return
                ctx
                |> render
                       (DetailsTemplate.Template details permissions)
                       (DetailsTemplate.FullPage details permissions username)
        }
    
let import: EndpointHandler =
    fun ctx -> task {
        let username = ctx.UserName
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
            route "/summaries/mailing-list" (mailingList dependencies.ReadMailingList)
            routef "/{%d}" (details dependencies.ReadOrganizationDetailsBy)
            route "/import" import
          ]
      POST [
          route "/import/upload" (upload dependencies.Import)
      ]
      subRoute "/" (SectionsRouter.Endpoints dependencies)
    ]
