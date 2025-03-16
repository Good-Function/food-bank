module Organizations.Templates.WarunkiPomocy

open Layout
open Organizations.Application
open Organizations.Templates.Fields
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations.Templates.Formatters

let View (warunki: ReadModels.WarunkiPomocy) (teczka: int64) =
    article () {
        header (class' = "action-header") {
            span () { "Warunki udzielania pomocy żywnościowej" }

            div (class' = "action-header-actions") {
                span (
                    hxGet = $"/organizations/{teczka}/warunki-pomocy/edit",
                    hxTarget = "closest article",
                    hxSwap = "outerHTML"
                ) {
                    Icons.Pen
                }
            }
        }

        field "Kategoria" warunki.Kategoria
        field "Rodzaj pomocy" warunki.RodzajPomocy
        field "Sposób udzielania pomocy" warunki.SposobUdzielaniaPomocy
        field "Warunki magazynowe" warunki.WarunkiMagazynowe
        field "HACCP" (warunki.HACCP |> toTakNie)
        field "Sanepid" (warunki.Sanepid |> toTakNie)
        field "Transport - opis" warunki.TransportOpis
        field "Transport - kategoria" warunki.TransportKategoria
    }

let Form (warunki: ReadModels.WarunkiPomocy) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            header (class' = "action-header") {
                span () { "Warunki udzielania pomocy żywnościowej" }

                div (class' = "action-header-actions") {
                    span (
                        hxGet = $"/organizations/{teczka}/waurnki-pomocy",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Cancel
                    }

                    span (
                        hxPut = $"/organizations/{teczka}/warunki-pomocy",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Ok
                    }
                }
            }
            
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
