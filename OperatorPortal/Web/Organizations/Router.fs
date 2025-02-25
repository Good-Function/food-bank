module Organizations.Router

open Oxpecker
open RenderBasedOnHtmx
open Organizations.Application.ReadModels
open Organizations.CompositionRoot

let indexPage: EndpointHandler =
    fun ctx ->
        ctx.WriteHtmlView (SearchableListTemplate.FullPage "")

let list: EndpointHandler =
    fun ctx ->
        task {
            let search =
                match ctx.TryGetQueryValue "search" with
                | None -> ""
                | Some s -> s
            return ctx |> render (SearchableListTemplate.Template search) (SearchableListTemplate.FullPage search)
        }
        
let summaries (readSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let search = ctx.TryGetQueryValue "search" |> Option.defaultValue ""
            let! summaries = readSummaries search
            return ctx.WriteHtmlView (ListTemplate.Template summaries)
        }

let details (readDetailsBy: ReadOrganizationDetailsBy) (id: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy id

            return
                ctx
                |> render (DetailsTemplate.Template details) (DetailsTemplate.FullPage details)
        }

let Endpoints (dependencies: Dependencies) =
    [ route "/" indexPage
      route "/list" list
      route "/summaries" (summaries dependencies.ReadOrganizationSummaries)
      routef "/{%d}" (details dependencies.ReadOrganizationDetailsBy) ]
