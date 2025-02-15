module Applications.Template

open Layout.Navigation
open Oxpecker.ViewEngine
open Layout

let private page (testList: string list) =
    div () {
        "Applications Page"

        for row in testList do
            h2 () { row }
    }

let Partial (testList: string list) =
    Fragment() {
        Body.Template (page testList) Page.Applications
        title () { Page.Applications.ToTitle() }
    }

let FullPage (testList: string list) =
    Head.Template (Partial testList) (Page.Applications.ToTitle())
