module Configuration.ResetPassword

open Layout
open Oxpecker.ViewEngine

let private page =
    div () {
       "Zmiana hasła"
    }

let Partial =
    Fragment() {
        Body.Template page None
        "Zmiana hasła" |> Head.ReplaceTitle 
    }

let FullPage  =
    Head.Template Partial "Zmiana hasła"