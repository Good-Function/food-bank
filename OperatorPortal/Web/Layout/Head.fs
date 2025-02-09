module Layout.Head

open Oxpecker.ViewEngine
open Oxpecker.Htmx

let ReplaceTitle (str: string) =
    title (id = "title", hxSwapOob = "true") { str + " | Bzsos" }

let Template (content: HtmlElement) (currentTitle: string) =
    html (lang = "pl") {
        head () {
            meta (charset = "utf-8")
            meta (name = "viewport", content = "width=device-width, initial-scale=1")
            meta (name = "color-scheme", content = "light dark")
            meta (name = "htmx-config", content="""{"responseHandling": [{"code":".*", "swap": true}]}""")
            script (src = "/htmx.min.js")
            script (src = "/theme.js")
            link (rel = "stylesheet", href = "/custom-styles.css")
            link (rel = "stylesheet", href = "/pico.pumpkin.min.css")
            title (id="title") { currentTitle + " | Bzsos" }
        }
        content
    }