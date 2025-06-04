module Web.Layout.Dropdown

open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

let DropDown(width: int) (icon: RawTextNode) (content: HtmlElement)=
    div(class'="dropdown", tabindex= -1) {
        i(class'="db2", tabindex= -1)
        a(style = $"width:{width}px", class'="dropbtn") {
            icon
        }
        div(class'="dropdown-content", role="menu") {
            content
        }
    }