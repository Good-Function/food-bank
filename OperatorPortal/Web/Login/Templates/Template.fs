module Login.Template

open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Oxpecker.Htmx
open Layout

let private login (returnUrl: string option) (antiforgeryToken: HtmlElement) =
    let actionWithReturnUrl =
        match returnUrl with
                | Some url -> $"/login?ReturnUrl={url}"
                | None -> "/login"
    body (class' = "container", hxBoost=true) {
        header () {
            div (style = "display: flex; align-items: center; justify-content: space-between;") {
                div () { "" }
                img (src = "/img/bzsoslogo.png", style = "width:175px;")
                ThemeToggler.Component
            }
            hr ()
        }

        main (style = "max-width:450px; margin:auto") {
            form (action = actionWithReturnUrl, method = "POST", hxTarget="#ErrorContainer", hxIndicator="#spinner" ) {
                antiforgeryToken
                input (
                    autocomplete = "email",
                    ariaLabel = "Email",
                    placeholder = "Email",
                    name = "Email",
                    type' = "Email",
                    required=true
                )
                input (
                    ariaLabel = "Password",
                    placeholder = "Password",
                    name = "Password",
                    type' = "password",
                    required=true
                )
                input (type'="submit", name="Login", value="Zaloguj")
                div(id="ErrorContainer")
                div(id="spinner", class'="htmx-indicator", style="text-align:center; height:50px;") {
                    Icons.Spinner
                }
            }
        }
    }
let LoginTemplate url = Head.Template (login url (Fragment())) "Logowanie"
let LoginTemplateWithToken returnUrl token = Head.Template (login returnUrl token) "Logowanie"
let LoginError = span(style="color: var(--pico-del-color)") { "Invalid Email or Password." }