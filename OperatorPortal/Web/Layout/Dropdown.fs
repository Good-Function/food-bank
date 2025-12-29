module Layout.Dropdown

open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

type Placement =
    | Bottom
    | Custom of string
    | Left
    override this.ToString() =
        match this with
        | Bottom -> "dropdown-bottom"
        | Custom  _-> "dropdown-bottom"
        | Left -> "dropdown-left"

let DropDown (width: int, icon: HtmlElement) (placement: Placement, content: HtmlElement)=
    div(class'="dropdown", tabindex= -1) {
        i(class'="db2", tabindex= -1)
        a(style = $"width:{width}px", class'="dropbtn") {
            icon
        }
        match placement with
        | Bottom | Left -> 
            div(class' = $"dropdown-content {placement.ToString()}", role="menu") {
                content
            }
        | Custom positionText ->
            div(class' = "dropdown-content", style=positionText , role="menu") {
                content
            }
    }