module Users.Index

open Layout.Head
open Layout.Navigation
open Oxpecker.ViewEngine
open Layout
open Oxpecker.Htmx

let private page =
    Fragment() {
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
    }

let Partial (userName: string) =
    Fragment() {
        Body.Template page (Some Page.Team) userName
        ReplaceTitle <| Page.Team.ToTitle()
    }

let FullPage (userName: string) =
    Head.Template (Partial userName) (Page.Team.ToTitle())
