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

        field "Wniosek" (dokumenty.Wniosek |> formatDate)
        field "Umowa z dnia" (dokumenty.UmowaZDn |> formatDate)
        field "Umowa z RODO" (dokumenty.UmowaRODO |> formatDate)
        field "Karty organizacji" (dokumenty.KartyOrganizacjiData |> formatDate)
        field "Ostatnie odwiedziny" (dokumenty.OstatnieOdwiedzinyData |> formatDate)
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

            editField "Wniosek" (dokumenty.Wniosek |> formatDate) (nameof dokumenty.Wniosek)
            editField "Umowa z dnia" (dokumenty.UmowaZDn |> formatDate) (nameof dokumenty.UmowaZDn)
            editField "Umowa z RODO" (dokumenty.UmowaRODO |> formatDate) (nameof dokumenty.UmowaRODO)
            editField
                "Karty organizacji"
                (dokumenty.KartyOrganizacjiData |> formatDate)
                (nameof dokumenty.KartyOrganizacjiData)
            editField
                "Ostatnie odwiedziny"
                (dokumenty.OstatnieOdwiedzinyData |> formatDate)
                (nameof dokumenty.OstatnieOdwiedzinyData)
        }
    }
