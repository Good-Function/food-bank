module Organizations.Templates.WarunkiPomocy

open Layout
open Organizations.Application
open Organizations.Templates.Fields
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations.Templates.Formatters

let View (warunki: ReadModels.WarunkiPomocy) (teczka: int64) =
    article () {
        editableHeader "Warunki udzielania pomocy żywnościowej"  $"/organizations/{teczka}/warunki-pomocy/edit"
        readonlyField "Kategoria" warunki.Kategoria
        readonlyField "Rodzaj pomocy" warunki.RodzajPomocy
        readonlyField "Sposób udzielania pomocy" warunki.SposobUdzielaniaPomocy
        readonlyField "Warunki magazynowe" warunki.WarunkiMagazynowe
        readonlyField "HACCP" (warunki.HACCP |> toTakNie)
        readonlyField "Sanepid" (warunki.Sanepid |> toTakNie)
        readonlyField "Transport - opis" warunki.TransportOpis
        readonlyField "Transport - kategoria" warunki.TransportKategoria
    }

let Form (warunki: ReadModels.WarunkiPomocy) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Warunki udzielania pomocy żywnościowej" $"/organizations/{teczka}/waurnki-pomocy"
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
