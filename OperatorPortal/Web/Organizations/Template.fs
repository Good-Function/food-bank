module Organizations.Template

open Layout.Navigation
open Oxpecker.ViewEngine
open Layout

let private page =
    div () { "Organizations Page" }
    
let Partial =
    Fragment() {
        Body.Template page Page.Organizations
        Head.ReplaceTitle <| Page.Organizations.ToTitle()
    }

let FullPage =
    Head.Template Partial (Page.Organizations.ToTitle())
