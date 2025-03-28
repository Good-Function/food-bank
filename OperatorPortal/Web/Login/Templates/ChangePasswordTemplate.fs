module Login.ChangePasswordTemplate

open Layout
open Layout.Fields
open Login.Domain
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let Form (errors: PasswordChangeErrors list)=
    let tryFindError errorType = errors |> List.tryFind ((=) errorType) |> Option.map _.ToMessage()
    form (style="max-width:650px; margin:auto;", hxPost="/login/password-change") {
       passwordField "Stare hasło" "OldPassword" (tryFindError OldPasswordIsIncorrect)
       passwordField "Nowe hasło" "NewPassword" (tryFindError NewPasswordIsTooWeak)
       passwordField "Powtórz nowe hasło" "NewPasswordConfirmation" (tryFindError ConfirmationIsIncorrect)
       input (type'="submit", name="Zapisz", value="Zmień hasło")
    }

let Partial (userName: string)  =
    Fragment() {
        Body.Template (Form []) None userName
        "Zmiana hasła" |> Head.ReplaceTitle
    }

let FullPage (userName: string)  =
    Head.Template (Partial userName) "Zmiana hasła"
    
let Success =
    h1(style="text-align:center") {
        div() { "Hasło zostało zmienione" }
        div() { "✅" }
    }

    