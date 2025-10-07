module Layout.Body

open Layout
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Layout.Dropdown

let Template (content: HtmlElement) (currentPath: Navigation.Page option) (userName: string) =
    body (hxBoost = true) {
        header (
            class' = "fluid-container",
            style="z-index:1; padding:8px 8px 0px 8px; box-shadow: 0px 0px 7px var(--pico-accordion-open-summary-color); margin-bottom: var(--pico-spacing); background: var(--pico-background-color); top:0; position:sticky;") {
            div (class'="container", style = "display: flex; align-items: flex-start; justify-content: space-between;") {
                div (title="v0.0.1", style="width:50px;") {
                    Icons.Logo
                }
                Navigation.Template currentPath
                div(style="display: inline-flex; flex-direction: column; vertical-align:top; text-align:right;") {   
                    div () {
                        DropDown (40, Icons.Profile) (Placement.Bottom, ProfilePopover.Template userName)
                        ThemeToggler.Component
                    }
                }
            }
        }
        main (class' = "container") { content }
        script (src = "/copyEmails.js")
    }