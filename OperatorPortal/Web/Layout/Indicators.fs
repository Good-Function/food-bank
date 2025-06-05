module Layout.Indicators

open Oxpecker.ViewEngine

let OverlaySpinner (id: string) =
    div (
        id = id,
        class' = "htmx-indicator",
        style =
            "position: absolute; z-index: 1; left: 0; right: 0; top: 0; bottom: 0; background-color: color-mix(in srgb, var(--pico-background-color) 80%, transparent);"
    ) {
        div (style = "width:100px; margin:auto;") { Icons.Spinner }
    }

let TableShimmeringRows amount colspan =
    tbody(class' = "table-indicator") {
        for _ in 1..amount do
            tr () { td (colspan = colspan) { div (class' = "shimmer", style = "padding-bottom:3px;") } }
    }
