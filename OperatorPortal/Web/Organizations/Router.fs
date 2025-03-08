module Organizations.Router

open Organizations.Application
open Organizations.Dtos
open Oxpecker
open RenderBasedOnHtmx
open Organizations.Application.ReadModels
open Organizations.CompositionRoot

let indexPage: EndpointHandler =
    _.WriteHtmlView(SearchableListTemplate.FullPage "" "")

let list: EndpointHandler =
    fun ctx ->
        task {
            let search =
                match ctx.TryGetQueryValue "search" with
                | None -> ""
                | Some s -> s

            let orderBy=
                match ctx.TryGetQueryValue "orderBy" with
                | None -> ""
                | Some s -> s

            printfn "OrderBy value: %s" orderBy

            return
                ctx
                |> render (SearchableListTemplate.Template search orderBy) (SearchableListTemplate.FullPage search orderBy)
        }

let summaries (readSummaries: ReadOrganizationSummaries) : EndpointHandler =
    fun ctx ->
        task {
            let search = ctx.TryGetQueryValue "search" |> Option.defaultValue ""
            let orderBy = ctx.TryGetQueryValue "orderBy" |> Option.defaultValue "teczka"
            printfn "OrderBy value: %s" orderBy
            let! summaries = readSummaries (search, orderBy)
            return ctx.WriteHtmlView(ListTemplate.Template summaries)
        }

let daneAdresowe (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(DaneAdresowe.View details.DaneAdresowe teczka)
        }

let daneAdresoweEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(DaneAdresowe.Form details.DaneAdresowe teczka)
        }

let changeDaneAdresowe (handle: Commands.ChangeDaneAdresowe) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<DaneAdresoweForm>()
            let cmd = form.toChangeDaneAdresowe teczka
            do! cmd |> handle
            return ctx.WriteHtmlView(DaneAdresowe.View form.toDaneAdresowe teczka)
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
    [ GET
          [ route "/" indexPage
            route "/list" list
            route "/summaries" (summaries dependencies.ReadOrganizationSummaries)
            routef "/{%d}" (details dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/dane-adresowe" (daneAdresowe dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/dane-adresowe/edit" (daneAdresoweEdit dependencies.ReadOrganizationDetailsBy) ]
      PUT [ routef "/{%d}/dane-adresowe" (changeDaneAdresowe dependencies.ModifyDaneAdresowe)] ]
