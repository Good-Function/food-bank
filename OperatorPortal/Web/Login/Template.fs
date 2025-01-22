module Login.Template

open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Oxpecker.Htmx
open Layout

let private login (returnUrl: string option) =
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
            form (action = actionWithReturnUrl, method = "POST", hxTarget="#ErrorContainer" ) {
                input (
                    autocomplete = "email",
                    ariaLabel = "Email",
                    placeholder = "Email",
                    name = "Email",
                    type' = "Email"
                )
                input (ariaLabel = "Password", placeholder = "Password", name = "Password", type' = "password")
                input (type'="submit", name="Login", value="Zaloguj")
                div(id="ErrorContainer") 
            }
        }
    }
let LoginTemplate url = Head.Template (login url) "Logowanie"
let LoginError = span(style="color: var(--pico-del-color)") { "Invalid Email or Password." }