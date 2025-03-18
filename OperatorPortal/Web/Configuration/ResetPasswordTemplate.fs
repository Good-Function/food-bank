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
        "Ustawienia" |> Head.ReplaceTitle 
    }

let FullPage  =
    Body.Template Partial None