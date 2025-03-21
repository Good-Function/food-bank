module Organizations.Templates.AdresyKsiegowosci

open Organizations.Application
open Layout.Fields
open Oxpecker.ViewEngine

let View (adresy: ReadModels.AdresyKsiegowosci) (teczka: int64) =
    article () {
        editableHeader "Dane adresowe księgowości" $"/organizations/{teczka}/adresy-ksiegowosci/edit"
        readonlyField "Organizacja na którą wystawiamy WZ" adresy.NazwaOrganizacjiKsiegowanieDarowizn
        readonlyField "Adres" adresy.KsiegowanieAdres
        readonlyField "Telefon" adresy.TelOrganProwadzacegoKsiegowosc
    }

let Form (adresy: ReadModels.AdresyKsiegowosci) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Dane adresowe księgowości" $"/organizations/{teczka}/adresy-ksiegowosci"
            editField
                "Organizacja na którą wystawiamy WZ"
                adresy.NazwaOrganizacjiKsiegowanieDarowizn
                (nameof adresy.NazwaOrganizacjiKsiegowanieDarowizn)

            editField "Adres" adresy.KsiegowanieAdres (nameof adresy.KsiegowanieAdres)
            editField "Telefon" adresy.TelOrganProwadzacegoKsiegowosc (nameof adresy.TelOrganProwadzacegoKsiegowosc)
        }
    }
