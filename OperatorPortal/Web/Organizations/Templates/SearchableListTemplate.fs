module Organizations.SearchableListTemplate

open Layout.Navigation
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations
open PageComposer

let Template search =
    div (id = "OrganizationsPage") {
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

        div (id = "OrganizationsList", style = "overflow-x: scroll; max-height: 85vh") {
            small () {
                div (id = "OrganizationsList") {
                    div (
                        hxGet = "/organizations/summaries",
                        hxTrigger = "load",
                        hxTarget = "#OrganizationsList",
                        hxInclude = "[name='search']"
                    )

                    for _ in 1..6 do
                        div (class' = "shimmer")
                }
            }
        }
    }

let FullPage search =
    composeFullPage
        { Content = Template search
          CurrentPage = Page.Organizations }
