module Layout.Body

open Layout
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let Template (content: HtmlElement) (currentPath: Navigation.Page) =
    body (class' = "container", hxBoost = true) {
        header () {
            div (style = "display: flex; align-items: center; justify-content: space-between;") {
                div ( class'="bzsos-logo" )
                Navigation.Template currentPath
                div () { ThemeToggler.Component }
            }
            hr ()
        }
        main () { content }
    }