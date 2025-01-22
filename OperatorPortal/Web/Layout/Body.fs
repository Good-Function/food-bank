module Layout.Body

open Layout
open Navigation
open Oxpecker.ViewEngine

let Template (content: HtmlElement) (currentPath: Page) =
    body (class' = "container", id="container") {
        header () {
            div (style = "display: flex; align-items: center; justify-content: space-between;") {
                div (
                    style =
                        "background-image: url('/img/bzsoslogo.png'); background-size: contain; no-repeat; background-position: center; width: 195px; height: 80px;"
                )
                Template currentPath
                div (style = "width:175px;") { ThemeToggler.Component }
            }
            hr ()
        }
        main () { content }
    }