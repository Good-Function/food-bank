module Organizations.Templates.Beneficjenci

open Layout
open Organizations.Application
open Organizations.Templates.Fields
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let View (beneficjenci: ReadModels.Beneficjenci) (teczka: int64) =
    article () {
        header (class' = "action-header") {
            span () { "Kontakty" }

            div (class' = "action-header-actions") {
                span (
                    hxGet = $"/organizations/{teczka}/beneficjenci/edit",
                    hxTarget = "closest article",
                    hxSwap = "outerHTML"
                ) {
                    Icons.Pen
                }
            }
        }

        field "Liczba Beneficjentów" $"{beneficjenci.LiczbaBeneficjentow}"
        field "Beneficjenci" beneficjenci.Beneficjenci
    }

let Form (beneficjenci: ReadModels.Beneficjenci) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            header (class' = "action-header") {
                span () { "Beneficjenci" }

                div (class' = "action-header-actions") {
                    span (
                        hxGet = $"/organizations/{teczka}/beneficjenci",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Cancel
                    }

                    span (
                        hxPut = $"/organizations/{teczka}/beneficjenci",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Ok
                    }
                }
            }

            editField "Liczba Beneficjentów" $"{beneficjenci.LiczbaBeneficjentow}" (nameof beneficjenci.LiczbaBeneficjentow)
            editField "Beneficjenci" beneficjenci.Beneficjenci (nameof beneficjenci.Beneficjenci)
        }
    }
