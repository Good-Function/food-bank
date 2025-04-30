module Organizations.Templates.Dokumenty

open Organizations.Application
open Layout.Fields
open Oxpecker.ViewEngine
open Organizations.Templates.Formatters

let View (dokumenty: ReadModels.Dokumenty) (teczka: int64) =
    article () {
        editableHeader "Dokumenty" $"/organizations/{teczka}/dokumenty/edit"
        readonlyField "Wniosek" (dokumenty.Wniosek |> toDisplay)
        readonlyField "Umowa z dnia" (dokumenty.UmowaZDn |> toDisplay)
        readonlyField "Umowa z RODO" (dokumenty.UmowaRODO |> toDisplay)
        readonlyField "Karty organizacji" (dokumenty.KartyOrganizacjiData |> toDisplay)
        readonlyField "Ostatnie odwiedziny" (dokumenty.OstatnieOdwiedzinyData |> toDisplay)
        readonlyField "Upoważnienie do odbioru" (dokumenty.DataUpowaznieniaDoOdbioru |> toDisplay)
    }

let Form (dokumenty: ReadModels.Dokumenty) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Dokumenty" $"/organizations/{teczka}/dokumenty"
            dateField "Wniosek" dokumenty.Wniosek (nameof dokumenty.Wniosek)
            dateField "Umowa z dnia" dokumenty.UmowaZDn (nameof dokumenty.UmowaZDn)
            dateField "Umowa z RODO" dokumenty.UmowaRODO (nameof dokumenty.UmowaRODO)
            dateField
                "Karty organizacji"
                dokumenty.KartyOrganizacjiData
                (nameof dokumenty.KartyOrganizacjiData)
            dateField
                "Ostatnie odwiedziny"
                dokumenty.OstatnieOdwiedzinyData
                (nameof dokumenty.OstatnieOdwiedzinyData)
            dateField
                "Upoważnienie do odbioru"
                dokumenty.DataUpowaznieniaDoOdbioru
                (nameof dokumenty.DataUpowaznieniaDoOdbioru)
        }
    }
