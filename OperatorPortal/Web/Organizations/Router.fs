module Organizations.Router

open Oxpecker
open RenderBasedOnHtmx
open Organizations.Application.ReadModels
open Organizations.CompositionRoot

let renderPage = render PageTemplate.Partial PageTemplate.FullPage

let renderSummaries (readSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let! summaries = readSummaries
            return ctx.WriteHtmlView(ListTemplate.Template summaries)
        }

let renderDetails (readDetailsBy: ReadOrganizationDetailsBy) (id: int) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy id
            return ctx.WriteHtmlView(DetailsTemplate.Template details)
        }

let Endpoints (dependencies: Dependencies) =
    [ route "/" renderPage
      route "/list" (renderSummaries dependencies.ReadOrganizationSummaries)
      routef "/{%i}" (renderDetails dependencies.ReadOrganizationDetailsBy) ]
