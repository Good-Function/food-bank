module Applications.Template

open Layout.Navigation
open Oxpecker.ViewEngine
open Layout

let private page =
    div() {
        "Applications Page"
    }
    
let Partial =
    Fragment() {
        Body.Template page Page.Applications
        Head.ReplaceTitle <| Page.Applications.ToTitle()
    }
    
let FullPage =
    Head.Template Partial (Page.Applications.ToTitle())