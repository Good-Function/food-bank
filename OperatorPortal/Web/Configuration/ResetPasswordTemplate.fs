module Configuration.ResetPassword

open Layout
open Layout.Fields
open Oxpecker.ViewEngine

let private page=
    form (style="max-width:650px; margin:auto;", action = "/settings/password-reset", method = "POST") {
       passwordField "Stare hasło" "OldPassword"
       passwordField "Nowe hasło" "NewPassword"
       passwordField "Powtórz nowe hasło" "NewPasswordConfirmation"
       input (type'="submit", name="Zapisz", value="Zmień hasło")
    }

let Partial  =
    Fragment() {
        Body.Template page None
        "Zmiana hasła" |> Head.ReplaceTitle
    }

let FullPage  =
    Head.Template Partial "Zmiana hasła"