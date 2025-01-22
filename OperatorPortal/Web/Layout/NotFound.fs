module Layout.NotFound

open Oxpecker.ViewEngine

let Template =
    div (style = "text-align:center") {
        img (src = "https://fonts.gstatic.com/s/e/notoemoji/latest/1f440/512.gif", width = 256, height = 256)
        h1 () { "Not Found" }
        h4 () { a (href = "/") { "<-Home" } }
    }
