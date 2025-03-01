module Organizations.DaneAdresowe

open Layout
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let private editField (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        input (value = value)
    }

let private field (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        small () { value }
    }

let View (org: OrganizationDetails) =
    article () {
        header (class' = "action-header") {
            span () { "Dane adresowe" }

            div (class' = "action-header-actions") {
                span (
                    hxGet = $"/organizations/{org.Teczka}/dane-adresowe/edit",
                    hxTarget = "closest article",
                    hxSwap = "outerHTML"
                ) {
                    Icons.Pen
                }
            }
        }

        field "Organizacja, która podpisała umowę" org.NazwaOrganizacjiPodpisujacejUmowe
        field "Adres rejestrowy" org.AdresRejestrowy
        field "Placówka do której trafia żywność" org.NazwaPlacowkiTrafiaZywnosc
        field "Adres dostawy żywności" org.AdresPlacowkiTrafiaZywnosc
        field "Gmina / Dzielnica" org.GminaDzielnica
        field "Powiat" org.Powiat
    }

let Form (org: OrganizationDetails) =
    form () {
        article (class' = "focus-dim") {
            header (class' = "action-header") {
                span () { "Dane adresowe" }

                div (class' = "action-header-actions") {
                    span (
                        hxGet = $"/organizations/{org.Teczka}/dane-adresowe",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Cancel
                    }

                    span (
                        hxPut = $"/organizations/{org.Teczka}/dane-adresowe",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Ok
                    }
                }
            }

            editField "Organizacja, która podpisała umowę" org.NazwaOrganizacjiPodpisujacejUmowe
            editField "Adres rejestrowy" org.AdresRejestrowy
            editField "Placówka do której trafia żywność" org.NazwaPlacowkiTrafiaZywnosc
            editField "Adres dostawy żywności" org.AdresPlacowkiTrafiaZywnosc
            editField "Gmina / Dzielnica" org.GminaDzielnica
            editField "Powiat" org.Powiat
        }
    }
