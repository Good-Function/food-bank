module Organizations.Templates.Beneficjenci

open Layout
open Organizations.Application
open Layout.Fields
open Oxpecker.ViewEngine

let View (beneficjenci: ReadModels.Beneficjenci) (teczka: int64) =
    article () {
        editableHeader "Beneficjenci" $"/organizations/{teczka}/beneficjenci/edit"
        readonlyField "Liczba Beneficjentów" $"{beneficjenci.LiczbaBeneficjentow}"
        readonlyField "Beneficjenci" beneficjenci.Beneficjenci
    }

let Form (beneficjenci: ReadModels.Beneficjenci) (teczka: int64) =
    let indicator = "BeneficjenciSpinner"
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Beneficjenci" $"/organizations/{teczka}/beneficjenci" indicator
            Indicators.OverlaySpinner indicator
            editField "Liczba Beneficjentów" $"{beneficjenci.LiczbaBeneficjentow}" (nameof beneficjenci.LiczbaBeneficjentow)
            editField "Beneficjenci" beneficjenci.Beneficjenci (nameof beneficjenci.Beneficjenci)
        }
    }
