module Layout.Body

open Layout
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let Template (content: HtmlElement) (currentPath: Navigation.Page option) (userName: string) =
    body (class' = "container", hxBoost = true) {
        header () {
            div (style = "display: flex; align-items: center; justify-content: space-between;") {
                div ( class'="bzsos-logo" )
                Navigation.Template currentPath
                div(style="display: inline-flex; flex-direction: column; vertical-align:top; text-align:right;") {   
                    div () {
                        button(id="ProfilePopoverTrigger").attr("popovertarget", "ProfilePopover") {
                            Icons.Profile
                        }
                        ProfilePopover.Template
                        ThemeToggler.Component
                    }
                    small(style="color: var(--pico-contrast-underline)") { userName }
                }
            }
            hr ()
        }
        main () { content }
    }