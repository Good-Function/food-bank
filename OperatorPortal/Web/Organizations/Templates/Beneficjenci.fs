module Organizations.Templates.Beneficjenci

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
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Beneficjenci" $"/organizations/{teczka}/beneficjenci"
            editField "Liczba Beneficjentów" $"{beneficjenci.LiczbaBeneficjentow}" (nameof beneficjenci.LiczbaBeneficjentow)
            editField "Beneficjenci" beneficjenci.Beneficjenci (nameof beneficjenci.Beneficjenci)
        }
    }
