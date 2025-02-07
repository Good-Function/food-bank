module Organizations.PageTemplate

open Layout.Navigation
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Layout

let private page =
    div (hxGet = "/organizations/list", hxTrigger = "load", id = "OrganizationsPage") {
        for _ in 1..6 do
            div (class' = "shimmer")
    }

let Partial =
    Fragment() {
        Body.Template page Page.Organizations
        Head.ReplaceTitle <| Page.Organizations.ToTitle()
    }

let FullPage = Head.Template Partial (Page.Organizations.ToTitle())
