module Users.Template

open Layout.Head
open Layout.Navigation
open Oxpecker.ViewEngine
open Layout

let private page =
    h1 () {
        "Zespół"
    }

let Partial (userName: string) =
    Fragment() {
        Body.Template (page) (Some Page.Applications) userName
        ReplaceTitle <| Page.Applications.ToTitle()
    }

let FullPage (userName: string) =
    Head.Template (Partial userName) (Page.Applications.ToTitle())
