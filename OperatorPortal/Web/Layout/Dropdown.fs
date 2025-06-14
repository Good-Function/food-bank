module Web.Layout.Dropdown

open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

type Placement =
    | Bottom
    | Left
    override this.ToString() =
        match this with
        | Bottom -> "dropdown-bottom"
        | Left -> "dropdown-left"

let DropDown (width: int, icon: HtmlElement) (placement: Placement, content: HtmlElement)=
    let placementClass = placement.ToString()
    div(class'="dropdown", tabindex= -1) {
        i(class'="db2", tabindex= -1)
        a(style = $"width:{width}px", class'="dropbtn") {
            icon
        }
        div(class' = $"dropdown-content {placementClass}", role="menu") {
            content
        }
    }