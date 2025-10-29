module Organizations.Templates.DaneAdresowe

open Layout
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Layout.Fields
open Permissions

let View (adresy: OrganizationDetails.DaneAdresowe) (teczka: int64) (permissions: Permission list) =
    article () {
        editableHeader2
            "Dane adresowe"
            $"/organizations/{teczka}/dane-adresowe/edit"
            permissions
            $"/organizations/{teczka}/audit-trail?kind=DaneAdresowe"
        readonlyField "Organizacja, która podpisała umowę" adresy.NazwaOrganizacjiPodpisujacejUmowe
        readonlyField "Adres rejestrowy" adresy.AdresRejestrowy
        readonlyField "Placówka do której trafia żywność" adresy.NazwaPlacowkiTrafiaZywnosc
        readonlyField "Adres dostawy żywności" adresy.AdresPlacowkiTrafiaZywnosc
        readonlyField "Gmina / Dzielnica" adresy.GminaDzielnica
        readonlyField "Powiat" adresy.Powiat
    }

let Form (adresy: OrganizationDetails.DaneAdresowe) (teczka: int64) =
    let indicator = "DaneAdresoweSpinner"
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Dane adresowe" $"/organizations/{teczka}/dane-adresowe" indicator
            Indicators.OverlaySpinner indicator
            editField "Organizacja, która podpisała umowę" adresy.NazwaOrganizacjiPodpisujacejUmowe (nameof adresy.NazwaOrganizacjiPodpisujacejUmowe)
            editField "Adres rejestrowy" adresy.AdresRejestrowy (nameof adresy.AdresRejestrowy)
            editField "Placówka do której trafia żywność" adresy.NazwaPlacowkiTrafiaZywnosc (nameof adresy.NazwaPlacowkiTrafiaZywnosc)
            editField "Adres dostawy żywności" adresy.AdresPlacowkiTrafiaZywnosc (nameof adresy.AdresPlacowkiTrafiaZywnosc)
            editField "Gmina / Dzielnica" adresy.GminaDzielnica (nameof adresy.GminaDzielnica)
            editField "Powiat" adresy.Powiat (nameof adresy.Powiat)
        }
    }
