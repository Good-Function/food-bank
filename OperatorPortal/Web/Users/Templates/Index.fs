module Users.Index

open Layout.Head
open Layout.Navigation
open Oxpecker.ViewEngine
open Layout
open Oxpecker.Htmx

let private page =
    Fragment() {
        form(style="margin:auto;",
             hxPost="/team/users",
             hxTarget="#UsersTable",
             hxIndicator="#UsersIndicator").attr("hx-on::after-request", "if(event.detail.successful) this.reset()") {
            fieldset().attr("role", "group") {
                input(
                    type'="email",
                    name="Email",
                    placeholder="Email nowej osoby")
                input(type' = "submit", value = "Wyślij zaproszenie", style="font-size:1rem !important;")
            }
        }
        div(id="UsersTable", style="position:relative"){
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

let Partial (userName: string) =
    Fragment() {
        Body.Template page (Some Page.Team) userName
        ReplaceTitle <| Page.Team.ToTitle()
    }

let FullPage (userName: string) =
    Head.Template (Partial userName) (Page.Team.ToTitle())
