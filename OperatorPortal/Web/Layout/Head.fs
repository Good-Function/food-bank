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
            meta (name = "description", content = "Portal operatora banku żywności - zarządzanie organizacjami, wnioskami i zespołem")
            meta (name = "color-scheme", content = "light dark")
            meta (name = "htmx-config", content="""{"responseHandling": [{"code":".*", "swap": true}]}""")
            link (rel = "preload", href = "/pico.pumpkin.min.css", as' = "style")
            link (rel = "preload", href = "/htmx.min.js", as' = "script")
            link (rel = "stylesheet", href = "/pico.pumpkin.min.css")
            link (rel = "stylesheet", href = "/ring-chart.css")
            link (rel = "stylesheet", href = "/dropdown.css")
            link (rel = "stylesheet", href = "/data-table.css")
            link (rel = "stylesheet", href = "/timeline.css")
            link (rel = "stylesheet", href = "/custom-styles.css")
            script (src = "/htmx.min.js")
            script (src = "/theme.js", defer = true)
            script () {}
            title (id="title") { currentTitle + " | Bzsos" }
        }
        content
    }