module Organizations.Template

open Layout.Navigation
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Layout

type OrganizationDto =
    { Name: string
      ContactPerson: string
      City: string }

let private page =
    div (hxGet = "/organizations?data=true", hxTrigger = "load") { "Loading..." }

let DataTemplate (data: OrganizationDto list) =
    div () {
        table (class' = "striped") {
            thead () {
                tr () {
                    th () { "Name" }
                    th () { "Contact person" }
                    th () { "City" }
                }
            }

            tbody () {
                for row in data do
                    tr () {
                        td () { row.Name }
                        td () { row.ContactPerson }
                        td () { row.City }
                    }
            }
        }
    }

let Partial =
    Fragment() {
        Body.Template page Page.Organizations
        Head.ReplaceTitle <| Page.Organizations.ToTitle()
    }

let FullPage = Head.Template Partial (Page.Organizations.ToTitle())
