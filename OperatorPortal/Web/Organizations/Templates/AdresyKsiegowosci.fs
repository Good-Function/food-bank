module Organizations.Templates.AdresyKsiegowosci

open Layout
open Organizations.Application
open Organizations.Templates.Fields
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let View (adresy: ReadModels.AdresyKsiegowosci) (teczka: int64) =
    article () {
        header (class' = "action-header") {
            span () { "Dane adresowe księgowości" }

            div (class' = "action-header-actions") {
                span (
                    hxGet = $"/organizations/{teczka}/adresy-ksiegowosci/edit",
                    hxTarget = "closest article",
                    hxSwap = "outerHTML"
                ) {
                    Icons.Pen
                }
            }
        }

        field "Organizacja na którą wystawiamy WZ" adresy.NazwaOrganizacjiKsiegowanieDarowizn
        field "Adres" adresy.KsiegowanieAdres
        field "Telefon" adresy.TelOrganProwadzacegoKsiegowosc
    }

let Form (adresy: ReadModels.AdresyKsiegowosci) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            header (class' = "action-header") {
                span () { "Dane adresowe księgowości" }

                div (class' = "action-header-actions") {
                    span (
                        hxGet = $"/organizations/{teczka}/adresy-ksiegowosci",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Cancel
                    }

                    span (
                        hxPut = $"/organizations/{teczka}/adresy-ksiegowosci",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Ok
                    }
                }
            }

            editField
                "Organizacja na którą wystawiamy WZ"
                adresy.NazwaOrganizacjiKsiegowanieDarowizn
                (nameof adresy.NazwaOrganizacjiKsiegowanieDarowizn)

            editField "Adres" adresy.KsiegowanieAdres (nameof adresy.KsiegowanieAdres)
            editField "Telefon" adresy.TelOrganProwadzacegoKsiegowosc (nameof adresy.TelOrganProwadzacegoKsiegowosc)
        }
    }
