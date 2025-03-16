module Organizations.Templates.DaneAdresowe

open Organizations.Application
open Oxpecker.ViewEngine
open Fields

let View (adresy: ReadModels.DaneAdresowe) (teczka: int64) =
    article () {
        editableHeader "Dane adresowe" $"/organizations/{teczka}/dane-adresowe/edit"
        readonlyField "Organizacja, która podpisała umowę" adresy.NazwaOrganizacjiPodpisujacejUmowe
        readonlyField "Adres rejestrowy" adresy.AdresRejestrowy
        readonlyField "Placówka do której trafia żywność" adresy.NazwaPlacowkiTrafiaZywnosc
        readonlyField "Adres dostawy żywności" adresy.AdresPlacowkiTrafiaZywnosc
        readonlyField "Gmina / Dzielnica" adresy.GminaDzielnica
        readonlyField "Powiat" adresy.Powiat
    }

let Form (adresy: ReadModels.DaneAdresowe) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Dane adresowe" $"/organizations/{teczka}/dane-adresowe"
            editField "Organizacja, która podpisała umowę" adresy.NazwaOrganizacjiPodpisujacejUmowe (nameof adresy.NazwaOrganizacjiPodpisujacejUmowe)
            editField "Adres rejestrowy" adresy.AdresRejestrowy (nameof adresy.AdresRejestrowy)
            editField "Placówka do której trafia żywność" adresy.NazwaPlacowkiTrafiaZywnosc (nameof adresy.NazwaPlacowkiTrafiaZywnosc)
            editField "Adres dostawy żywności" adresy.AdresPlacowkiTrafiaZywnosc (nameof adresy.AdresPlacowkiTrafiaZywnosc)
            editField "Gmina / Dzielnica" adresy.GminaDzielnica (nameof adresy.GminaDzielnica)
            editField "Powiat" adresy.Powiat (nameof adresy.Powiat)
        }
    }
