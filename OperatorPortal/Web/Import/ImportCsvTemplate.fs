module Import.ImportFileTemplate

open Layout
open Oxpecker.ViewEngine

let private page =
    div () {
        "Import File"
    }

let Partial (userName: string) =
    Fragment() {
        Body.Template page None userName
        "Import File" |> Head.ReplaceTitle 
    }

let FullPage (userName: string)  =
    Head.Template (Partial userName) "Import File"