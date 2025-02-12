module Web.Organizations.renderOrganizationPage

open Layout
open Layout.Navigation
open Oxpecker.ViewEngine

type PageOptions = {
    Content: HtmlElement
    CurrentPage: Page
}

let composePage (page: PageOptions) =
    Head.Template (Body.Template page.Content page.CurrentPage) (page.CurrentPage.ToTitle()) 