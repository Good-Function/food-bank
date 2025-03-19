module Organizations.Templates.ZrodlaZywnosci

open Organizations.Application
open Layout.Fields
open Oxpecker.ViewEngine
open Organizations.Templates.Formatters

let View (zrodla: ReadModels.ZrodlaZywnosci) (teczka: int64) =
    article () {
        editableHeader "Źródła żywności"  $"/organizations/{teczka}/zrodla-zywnosci/edit"
        readonlyField "Sieci" (zrodla.Sieci |> toTakNie)
        readonlyField "Bazarki" (zrodla.Bazarki |> toTakNie)
        readonlyField "Machfit" (zrodla.Machfit |> toTakNie)
        readonlyField "FEPŻ 2024" (zrodla.FEPZ2024 |> toTakNie)
    }

let Form (zrodla: ReadModels.ZrodlaZywnosci) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Źródła żywności" $"/organizations/{teczka}/zrodla-zywnosci"
            booleanField "Sieci" zrodla.Sieci (nameof zrodla.Sieci)
            booleanField "Bazarki" zrodla.Bazarki (nameof zrodla.Bazarki)
            booleanField "Machfit" zrodla.Machfit (nameof zrodla.Machfit)
            booleanField "FEPŻ 2024" zrodla.FEPZ2024 (nameof zrodla.FEPZ2024)
        }
    }
