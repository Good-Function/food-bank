module Organizations.Templates.ZrodlaZywnosci

open Layout
open Organizations.Application
open Organizations.Templates.Fields
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations.Templates.Formatters

let View (zrodla: ReadModels.ZrodlaZywnosci) (teczka: int64) =
    article () {
        header (class' = "action-header") {
            span () { "Źródła żywności" }

            div (class' = "action-header-actions") {
                span (
                    hxGet = $"/organizations/{teczka}/zrodla-zywnosci/edit",
                    hxTarget = "closest article",
                    hxSwap = "outerHTML"
                ) {
                    Icons.Pen
                }
            }
        }

        field "Sieci" (zrodla.Sieci |> toTakNie)
        field "Bazarki" (zrodla.Bazarki |> toTakNie)
        field "Machfit" (zrodla.Machfit |> toTakNie)
        field "FEPŻ 2024" (zrodla.FEPZ2024 |> toTakNie)
    }

let Form (zrodla: ReadModels.ZrodlaZywnosci) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            header (class' = "action-header") {
                span () { "Źródła żywności" }

                div (class' = "action-header-actions") {
                    span (
                        hxGet = $"/organizations/{teczka}/zrodla-zywnosci",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Cancel
                    }

                    span (
                        hxPut = $"/organizations/{teczka}/zrodla-zywnosci",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Ok
                    }
                }
            }

            booleanField "Sieci" zrodla.Sieci (nameof zrodla.Sieci)
            booleanField "Bazarki" zrodla.Bazarki (nameof zrodla.Bazarki)
            booleanField "Machfit" zrodla.Machfit (nameof zrodla.Machfit)
            booleanField "FEPŻ 2024" zrodla.FEPZ2024 (nameof zrodla.FEPZ2024)
        }
    }
