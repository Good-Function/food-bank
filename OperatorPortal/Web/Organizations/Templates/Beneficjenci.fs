module Organizations.Templates.Beneficjenci

open Layout
open Organizations.Application.ReadModels
open Layout.Fields
open Oxpecker.ViewEngine
open Permissions

let View (beneficjenci: OrganizationDetails.Beneficjenci) (teczka: int64) (permissions: Permission list) =
    article () {
        editableHeader2
            "Beneficjenci"
            $"/organizations/{teczka}/beneficjenci/edit"
            permissions
            $"/organizations/{teczka}/audit-trail?kind=beneficjenci"
        readonlyField "Liczba Beneficjentów" $"{beneficjenci.LiczbaBeneficjentow}"
        readonlyField "Beneficjenci" beneficjenci.Beneficjenci
    }

let Form (beneficjenci: OrganizationDetails.Beneficjenci) (teczka: int64) (antiforgeryToken: HtmlElement) =
    let indicator = "BeneficjenciSpinner"
    form () {
        antiforgeryToken
        article (class' = "focus-dim") {
            activeEditableHeader "Beneficjenci" $"/organizations/{teczka}/beneficjenci" indicator
            Indicators.OverlaySpinner indicator
            editField "Liczba Beneficjentów" $"{beneficjenci.LiczbaBeneficjentow}" (nameof beneficjenci.LiczbaBeneficjentow)
            editField "Beneficjenci" beneficjenci.Beneficjenci (nameof beneficjenci.Beneficjenci)
        }
    }
