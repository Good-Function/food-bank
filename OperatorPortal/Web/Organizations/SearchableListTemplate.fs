module Organizations.SearchableListTemplate

open Layout
open Layout.Navigation
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations
open renderOrganizationPage

let Template (search: string option) =
    div(id="OrganizationsPage") {
        input (
            type' = "search",
            name = "search",
            value = (search |> Option.defaultValue ""),
            title = "Szukaj po: Teczka, Nazwa placówki",
            hxGet = "/organizations/list",
            placeholder = "Szukaj po teczce, nazwie placówki...",
            hxTrigger = "input changed delay:500ms, keyup[key=='Enter']",
            hxTarget = "#OrganizationsList",
            hxPushUrl= "true",
            hxPreserve = true
        )

        div (class' = "overflow-auto") {
            small () {
                div (id = "OrganizationsList") {
                    div(hxGet = "/organizations/list", hxTrigger="load", hxTarget="#OrganizationsList", hxInclude="[name='search']")
                    for _ in 1..6 do
                        div (class' = "shimmer")
                }
            }
        }
    }
    
let PartialPage (search: string option) =
    Body.Template (Template search) Page.Organizations
    
let FullPage (search: string option) =
    composePage {Content = Template search; CurrentPage = Page.Organizations}