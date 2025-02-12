module Organizations.Router

open Oxpecker
open RenderBasedOnHtmx
open Organizations.Application.ReadModels
open Organizations.CompositionRoot

let renderPage: EndpointHandler =
    fun ctx ->
        ctx |> render (SearchableListTemplate.PartialPage None) (SearchableListTemplate.FullPage None)

let renderSummaries (readSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let search =
                match ctx.TryGetQueryValue "search" with
                | None -> ""
                | Some s -> s

            let! summaries = readSummaries
            let template =
                match search with
                | "" -> ListTemplate.Template summaries
                | s ->
                    ListTemplate.Template(
                        summaries
                        |> List.filter (fun sum ->
                            sum.Teczka.ToString().Contains(s) || sum.NazwaPlacowkiTrafiaZywnosc.ToLowerInvariant().Contains(s.ToLowerInvariant()))
                    )
            return ctx |> render template (SearchableListTemplate.FullPage (ctx.TryGetQueryValue "search"))
        }

let renderDetails (readDetailsBy: ReadOrganizationDetailsBy) (id: int) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy id

            return
                ctx
                |> render (DetailsTemplate.Template details) (DetailsTemplate.FullPage details)
        }

let Endpoints (dependencies: Dependencies) =
    [ route "/" renderPage
      route "/list" (renderSummaries dependencies.ReadOrganizationSummaries)
      routef "/{%i}" (renderDetails dependencies.ReadOrganizationDetailsBy) ]
