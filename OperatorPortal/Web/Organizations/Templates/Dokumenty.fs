module Organizations.Templates.Dokumenty

open Layout
open Organizations.Application
open Organizations.Templates.Fields
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations.Templates.Formatters

let View (dokumenty: ReadModels.Dokumenty) (teczka: int64) =
    article () {
        header (class' = "action-header") {
            span () { "Dokumenty" }

            div (class' = "action-header-actions") {
                span (
                    hxGet = $"/organizations/{teczka}/dokumenty/edit",
                    hxTarget = "closest article",
                    hxSwap = "outerHTML"
                ) {
                    Icons.Pen
                }
            }
        }

        field "Wniosek" (dokumenty.Wniosek |> toDisplay)
        field "Umowa z dnia" (dokumenty.UmowaZDn |> toDisplay)
        field "Umowa z RODO" (dokumenty.UmowaRODO |> toDisplay)
        field "Karty organizacji" (dokumenty.KartyOrganizacjiData |> toDisplay)
        field "Ostatnie odwiedziny" (dokumenty.OstatnieOdwiedzinyData |> toDisplay)
    }

let Form (dokumenty: ReadModels.Dokumenty) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            header (class' = "action-header") {
                span () { "Dokumenty" }

                div (class' = "action-header-actions") {
                    span (
                        hxGet = $"/organizations/{teczka}/dokumenty",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Cancel
                    }

                    span (
                        hxPut = $"/organizations/{teczka}/dokumenty",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Ok
                    }
                }
            }

            dateField "Wniosek" dokumenty.Wniosek (nameof dokumenty.Wniosek)
            dateField "Umowa z dnia" dokumenty.UmowaZDn (nameof dokumenty.UmowaZDn)
            dateField "Umowa z RODO" dokumenty.UmowaRODO (nameof dokumenty.UmowaRODO)
            dateField
                "Karty organizacji"
                dokumenty.KartyOrganizacjiData
                (nameof dokumenty.KartyOrganizacjiData)
            dateField
                "Ostatnie odwiedziny"
                dokumenty.OstatnieOdwiedzinyData
                (nameof dokumenty.OstatnieOdwiedzinyData)
        }
    }
