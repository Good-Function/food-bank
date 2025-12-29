module Organizations.Templates.PageComposer

open Layout
open Layout.Navigation
open Oxpecker.ViewEngine

type PageOptions = {
    Content: HtmlElement
    CurrentPage: Page
}

let composeFullPage (page: PageOptions) (userName: string) =
    Head.Template (Body.Template page.Content (Some page.CurrentPage) userName) (page.CurrentPage.ToTitle()) 