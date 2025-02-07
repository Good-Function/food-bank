module Layout.Body

open Layout
open Navigation
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let Template (content: HtmlElement) (currentPath: Page) =
    body (class' = "container", id="container", hxBoost = true) {
        header () {
            div (style = "display: flex; align-items: center; justify-content: space-between;") {
                div ( class'="bzsos-logo" )
                Template currentPath
                div () { ThemeToggler.Component }
            }
            hr ()
        }
        main () { content }
    }