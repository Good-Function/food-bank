module Users.Index

open Layout.Head
open Layout.Navigation
open Oxpecker.ViewEngine
open Layout
open Oxpecker.Htmx

let private page permissions (antiforgeryToken: HtmlElement)  =
    Fragment() {
        if permissions |> List.contains Permissions.ManageUsers then
            form(style="margin:auto;",
                 hxPost="/team/users",
                 hxTarget="#UsersTable",
                 hxIndicator="#UsersIndicator").attr("hx-on::after-request", "if(event.detail.successful) this.reset()") {
                antiforgeryToken
                fieldset().attr("role", "group") {
                    input(
                        type'="email",
                        name="Email",
                        placeholder="Email nowej osoby")
                    input(type' = "submit", value = "Wyślij zaproszenie", style="font-size:1rem !important;")
                }
            }
        else
            Fragment()
        div(id="UsersTable", style="position:relative; overflow:auto;"){
            div (hxGet = "/team/users", hxTrigger = "revealed", hxSwap = "outerHTML") {
                table(style="layout:fixed") {
                    thead() {
                        th(style="width:128px; min-width:64px;") {}
                        th(style="width:50%") {"Mail"}
                        th(style="min-width:140px;") {"Rola"}
                        th(style="width:128px") {}
                    }
                    Indicators.TableShimmeringRows 3
                }
            }
            Indicators.OverlaySpinner "UsersIndicator"
        }
    }

let Partial (userName: string) permissions (antiforgeryToken: HtmlElement)=
    Fragment() {
        Body.Template (page permissions antiforgeryToken) (Some Page.Team) userName
        ReplaceTitle <| Page.Team.ToTitle()
    }

let FullPage (userName: string) permissions (antiforgeryToken: HtmlElement) =
    Head.Template (Partial userName permissions antiforgeryToken) (Page.Team.ToTitle())
