module Organizations.Router

open System.Security.Claims
open Microsoft.AspNetCore.Http
open Organizations.Application
open Organizations.Templates
open Oxpecker
open RenderBasedOnHtmx
open Organizations.Application.ReadModels
open Organizations.CompositionRoot
open type Microsoft.AspNetCore.Http.TypedResults

// todo
// 1. Move this to organizations vertical slice ✅
// 2. Add Command handler which returns Result<Ok, Errors> ✅
// 3. Add proper validation (if closed xml fails, return info to UI that incorrect excel) 
// 4. Return errors as plain text
// 5. Add different error for headers

let indexPage: EndpointHandler =
    fun ctx ->
        let username = ctx.User.FindFirstValue(ClaimTypes.Name)
        ctx.WriteHtmlView(SearchableListTemplate.FullPage "" username)


let list: EndpointHandler =
    fun ctx ->
        task {
            let username = ctx.User.FindFirstValue(ClaimTypes.Name)
            let search = ctx.TryGetQueryValue "search" |> Option.defaultValue ""
            return
                ctx
                |> render (SearchableListTemplate.Template search) (SearchableListTemplate.FullPage search username)
        }

let summaries (readSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let search = ctx.TryGetQueryValue "search" |> Option.defaultValue ""
            let! summaries = readSummaries search
            return ctx.WriteHtmlView(ListTemplate.Template summaries)
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
        
let tryGetFirstFormFile (ctx: HttpContext) =
    if ctx.Request.HasFormContentType then
        ctx.Request.Form.Files |> Seq.tryHead
    else
        None
    
let import: EndpointHandler =
    fun ctx -> task {
        let username = ctx.User.FindFirstValue(ClaimTypes.Name)
        return ctx |> render (ImportFileTemplate.Partial username None) (ImportFileTemplate.FullPage username None)
    }
    
let upload (import: Commands.ImportOrganizations): EndpointHandler =
    fun ctx -> task {
        match tryGetFirstFormFile ctx with
        | Some file ->
            match import (file.OpenReadStream()) with
            | Ok output -> return! ctx.WriteText $"%A{output}"
            | Error err ->
                ctx.SetStatusCode(StatusCodes.Status400BadRequest)
                return! ctx.WriteHtmlView (ImportFileTemplate.Upload (Some $"%A{err}"))
        | None ->
            ctx.SetStatusCode(StatusCodes.Status400BadRequest)
            return! ctx.WriteHtmlView (ImportFileTemplate.Upload (Some "Niepoprawny plik excel (.xlsx)"))
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
