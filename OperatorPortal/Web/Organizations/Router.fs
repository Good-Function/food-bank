module Organizations.Router

open System
open System.Security.Claims
open Microsoft.AspNetCore.Http
open Organizations.Application
open Organizations.Application.ReadModels
open Organizations.List
open Organizations.Templates
open FsToolkit.ErrorHandling
open Oxpecker
open RenderBasedOnHtmx
open Organizations.CompositionRoot
open HttpContextExtensions
open type Microsoft.AspNetCore.Http.TypedResults

let replaceTextFilterOperator (operator: string option) =
    match operator with
    | Some "zawiera" -> Some "ILIKE"
    | Some "nie zawiera" -> Some "NOT ILIKE"
    | _ -> None

let parseFilter (ctx: HttpContext) : Query =
    let search = ctx.TryGetQueryValue "search" |> Option.defaultValue ""
    let sortBy = option {
        let! sortBy = ctx.TryGetQueryValue "sort"
        let! dir = ctx.TryGetQueryValue "dir"
        return sortBy, dir |> Direction.FromString
    }
    let benficjenci_op = ctx.TryGetQueryValue "Beneficjenci_op" |> replaceTextFilterOperator
    let benficjenci = ctx.TryGetQueryValue "Beneficjenci" 
    let liczba_beneficjentow_op = ctx.TryGetQueryValue "LiczbaBeneficjentow_op" 
    let liczba_beneficjentow = ctx.TryGetQueryValue "LiczbaBeneficjentow" 
                                    |> Option.bind (fun text ->
                                        let ok, value = Int32.TryParse text
                                        if ok then Some value else None
                                    )
    //todo todomg: Keys should be constant list to avoid SQL injection.
    let filters =
        [
            Option.map2 (fun value operator -> { Key = "LiczbaBeneficjentow"; Value = value; Operator = operator }) liczba_beneficjentow liczba_beneficjentow_op
            Option.map2 (fun value operator -> { Key = "Beneficjenci"; Value = value; Operator = operator }) benficjenci benficjenci_op
        ] |> List.choose id
    
    { SearchTerm = search; SortBy = sortBy; Filters = filters}

let indexPage: EndpointHandler =
    fun ctx ->
        let username = ctx.User.FindFirstValue(ClaimTypes.Name)
        ctx.WriteHtmlView(SearchableListTemplate.FullPage Query.Zero username)


let list: EndpointHandler =
    fun ctx ->
        task {
            let username = ctx.User.FindFirstValue(ClaimTypes.Name)
            let filter = ctx |> parseFilter
            return
                ctx
                |> render (SearchableListTemplate.Template filter) (SearchableListTemplate.FullPage filter username)
        }

let summaries (readSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let username = ctx.User.FindFirstValue(ClaimTypes.Name)
            let filter = ctx |> parseFilter
            let! summaries = readSummaries filter
            return ctx |> render (ListTemplate.Template summaries filter) (SearchableListTemplate.FullPage filter username)
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
            route "/list" list
            route "/summaries" (summaries dependencies.ReadOrganizationSummaries)
            routef "/{%d}" (details dependencies.ReadOrganizationDetailsBy)
            route "/import" import
          ]
      POST [
          route "/import/upload" (upload dependencies.Import)
      ]
      subRoute "/" (SectionsRouter.Endpoints dependencies)
    ]
