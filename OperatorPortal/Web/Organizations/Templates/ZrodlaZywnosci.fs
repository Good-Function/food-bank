module Organizations.Templates.ZrodlaZywnosci

open Layout
open Organizations.Application.ReadModels
open Layout.Fields
open Oxpecker.ViewEngine
open Organizations.Templates.Formatters

let View (zrodla: OrganizationDetails.ZrodlaZywnosci) (teczka: int64) =
    article () {
        editableHeader "Źródła żywności"  $"/organizations/{teczka}/zrodla-zywnosci/edit"
        readonlyField "Sieci" (zrodla.Sieci |> toTakNie)
        readonlyField "Bazarki" (zrodla.Bazarki |> toTakNie)
        readonlyField "Machfit" (zrodla.Machfit |> toTakNie)
        readonlyField "FEPŻ 2024" (zrodla.FEPZ2024 |> toTakNie)
        readonlyField "Odbiór Krótki Termin" (zrodla.OdbiorKrotkiTermin |> toTakNie)
        readonlyField "Tylko nasz magazyn" (zrodla.TylkoNaszMagazyn |> toTakNie)
    }

let Form (zrodla: OrganizationDetails.ZrodlaZywnosci) (teczka: int64) =
    let indicator = "ZrodlaZywnosciIndicator"
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Źródła żywności" $"/organizations/{teczka}/zrodla-zywnosci" indicator
            Indicators.OverlaySpinner indicator
            booleanField "Sieci" zrodla.Sieci (nameof zrodla.Sieci)
            booleanField "Bazarki" zrodla.Bazarki (nameof zrodla.Bazarki)
            booleanField "Machfit" zrodla.Machfit (nameof zrodla.Machfit)
            booleanField "FEPŻ 2024" zrodla.FEPZ2024 (nameof zrodla.FEPZ2024)
            booleanField "Odbiór Krótki Termin" zrodla.OdbiorKrotkiTermin (nameof zrodla.OdbiorKrotkiTermin)
            booleanField "Tylko nasz magazyn" zrodla.TylkoNaszMagazyn (nameof zrodla.TylkoNaszMagazyn)
        }
    }
