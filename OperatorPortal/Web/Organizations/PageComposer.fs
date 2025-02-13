module Web.Organizations.PageComposer

open Layout
open Layout.Navigation
open Oxpecker.ViewEngine

type PageOptions = {
    Content: HtmlElement
    CurrentPage: Page
}

let composeFullPage (page: PageOptions) =
    Head.Template (Body.Template page.Content page.CurrentPage) (page.CurrentPage.ToTitle()) 