module Configuration.ImportCsvTemplate

open Layout
open Oxpecker.ViewEngine

let private page =
    div () {
        "Import CSV"
    }

let Partial =
    Fragment() {
        Body.Template page None
        "Import CSV" |> Head.ReplaceTitle 
    }

let FullPage  =
    Body.Template Partial None