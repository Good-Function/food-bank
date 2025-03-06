module Organizations.DaneAdresowe

open Layout
open Organizations.Application
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let private editField (labelText: string) (value: string) (name: string) =
    p () {
        label () { b () { labelText } }
        input (value = value, name = name)
    }

let private field (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        small () { value }
    }

let View (adresy: ReadModels.DaneAdresowe) (teczka: int64) =
    article () {
        header (class' = "action-header") {
            span () { "Dane adresowe" }

            div (class' = "action-header-actions") {
                span (
                    hxGet = $"/organizations/{teczka}/dane-adresowe/edit",
                    hxTarget = "closest article",
                    hxSwap = "outerHTML"
                ) {
                    Icons.Pen
                }
            }
        }

        field "Organizacja, która podpisała umowę" adresy.NazwaOrganizacjiPodpisujacejUmowe
        field "Adres rejestrowy" adresy.AdresRejestrowy
        field "Placówka do której trafia żywność" adresy.NazwaPlacowkiTrafiaZywnosc
        field "Adres dostawy żywności" adresy.AdresPlacowkiTrafiaZywnosc
        field "Gmina / Dzielnica" adresy.GminaDzielnica
        field "Powiat" adresy.Powiat
    }

let Form (adresy: ReadModels.DaneAdresowe) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            header (class' = "action-header") {
                span () { "Dane adresowe" }

                div (class' = "action-header-actions") {
                    span (
                        hxGet = $"/organizations/{teczka}/dane-adresowe",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Cancel
                    }

                    span (
                        hxPut = $"/organizations/{teczka}/dane-adresowe",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Ok
                    }
                }
            }

            editField "Organizacja, która podpisała umowę" adresy.NazwaOrganizacjiPodpisujacejUmowe (nameof adresy.NazwaOrganizacjiPodpisujacejUmowe)
            editField "Adres rejestrowy" adresy.AdresRejestrowy (nameof adresy.AdresRejestrowy)
            editField "Placówka do której trafia żywność" adresy.NazwaPlacowkiTrafiaZywnosc (nameof adresy.NazwaPlacowkiTrafiaZywnosc)
            editField "Adres dostawy żywności" adresy.AdresPlacowkiTrafiaZywnosc (nameof adresy.AdresPlacowkiTrafiaZywnosc)
            editField "Gmina / Dzielnica" adresy.GminaDzielnica (nameof adresy.GminaDzielnica)
            editField "Powiat" adresy.Powiat (nameof adresy.Powiat)
        }
    }
