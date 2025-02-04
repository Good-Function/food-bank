module Organizations.Router

open Oxpecker
open RenderBasedOnHtmx
open Organizations.Application.ReadModels
open Organizations.CompositionRoot

let renderPage = render PageTemplate.Partial PageTemplate.FullPage

let renderOrganizations (readOrganizationSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let! summaries = readOrganizationSummaries
            return ctx.WriteHtmlView(
                ListTemplate.Template summaries
            )   
        }

let Endpoints (dependencies: Dependencies) =
    [
        route "/" renderPage
        route "/list" (renderOrganizations dependencies.ReadOrganizationSummaries)
    ]
