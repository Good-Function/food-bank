module Layout.ProfilePopover

open Oxpecker.ViewEngine

let Template (userName: string) =
    aside () {
        span(style="color: var(--pico-contrast-underline)") { userName }
        nav () {
            ul () {
                li (style = "display: block;") {
                    a (href = "/organizations/import") {
                        Icons.UploadFile
                        "Import csv"
                    }
                }
            }
        }
    }
