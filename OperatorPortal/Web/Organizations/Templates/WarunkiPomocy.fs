module Organizations.Templates.WarunkiPomocy

open Layout
open Organizations.Application.ReadModels
open Layout.Fields
open Oxpecker.ViewEngine
open Organizations.Templates.Formatters
open Permissions

let View (warunki: OrganizationDetails.WarunkiPomocy) (teczka: int64) (permissions: Permission list)=
    article () {
        editableHeader2
            "Warunki udzielania pomocy żywnościowej"
            $"/organizations/{teczka}/warunki-pomocy/edit"
            permissions $"/organizations/{teczka}/audit-trail?kind=WarunkiPomocy"
        readonlyField "Kategoria" warunki.Kategoria
        readonlyField "Rodzaj pomocy" warunki.RodzajPomocy
        readonlyField "Sposób udzielania pomocy" warunki.SposobUdzielaniaPomocy
        readonlyField "Warunki magazynowe" warunki.WarunkiMagazynowe
        readonlyField "HACCP" (warunki.HACCP |> toTakNie)
        readonlyField "Sanepid" (warunki.Sanepid |> toTakNie)
        readonlyField "Transport - opis" warunki.TransportOpis
        readonlyField "Transport - kategoria" warunki.TransportKategoria
    }

let Form (warunki: OrganizationDetails.WarunkiPomocy) (teczka: int64) =
    let indicator = "WarunkiPomocySpinner"
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Warunki udzielania pomocy żywnościowej" $"/organizations/{teczka}/warunki-pomocy" indicator
            Indicators.OverlaySpinner indicator
            editField "Kategoria" warunki.Kategoria (nameof warunki.Kategoria)
            editField "Rodzaj pomocy" warunki.RodzajPomocy (nameof warunki.RodzajPomocy)
            editField "Sposób udzielania pomocy" warunki.SposobUdzielaniaPomocy (nameof warunki.SposobUdzielaniaPomocy)
            editField "Warunki magazynowe" warunki.WarunkiMagazynowe (nameof warunki.WarunkiMagazynowe)
            booleanField "HACCP" warunki.HACCP (nameof warunki.HACCP)
            booleanField "Sanepid" warunki.Sanepid (nameof warunki.Sanepid)
            editField "Transport - opis" warunki.TransportOpis (nameof warunki.TransportOpis)
            editField "Transport - kategoria" warunki.TransportKategoria (nameof warunki.TransportKategoria)
        }
    }
