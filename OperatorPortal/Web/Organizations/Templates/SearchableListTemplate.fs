module Organizations.SearchableListTemplate

open Layout.Navigation
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations
open PageComposer

let Template search orderBy =
    div (id = "OrganizationsPage") {
        input (name = "orderBy", value = orderBy, type' = "hidden")
        input (
            type' = "search",
            name = "search",
            value = search,
            id = "OrganizationSearch",
            style = "transition:none;",
            title = "Szukaj po: Teczka, Nazwa placówki",
            hxGet = "/organizations/list",
            placeholder = "Szukaj po teczce, nazwie placówki...",
            hxTrigger = "input changed delay:500ms, keyup[key=='Enter']",
            hxTarget = "#OrganizationsPage",
            hxPushUrl = "true"
        )

        div (class' = "overflow-auto") {
            small () {
                div (id = "OrganizationsList") {
                    div (
                        hxGet = "/organizations/summaries",
                        hxTrigger = "load",
                        hxTarget = "#OrganizationsList",
                        hxInclude = "[name='search'], [name='orderBy']"
                    )

                    for _ in 1..6 do
                        div (class' = "shimmer")
                }
            }
        }
    }

let FullPage search orderBy =
    composeFullPage
        { Content = Template search orderBy
          CurrentPage = Page.Organizations }
