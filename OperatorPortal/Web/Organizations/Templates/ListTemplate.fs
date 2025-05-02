module Organizations.ListTemplate

open Organizations.Templates.Formatters
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Organizations.Application.ReadModels

let Template (data: OrganizationSummary list) =
    tbody () {
        for row in data do
            tr (style = "position: relative") {
                td () {
                    a (href = $"/organizations/%i{row.Teczka}", hxTarget = "#OrganizationsPage") { $"%i{row.Teczka}" }
                }

                td () { row.NazwaPlacowkiTrafiaZywnosc }
                td () { row.AdresPlacowkiTrafiaZywnosc }
                td () { row.GminaDzielnica }
                td () { row.FormaPrawna }
                td () { row.Telefon }
                td () { row.Email }
                td () { row.Kontakt }
                td () { row.OsobaDoKontaktu }
                td () { row.TelefonOsobyKontaktowej }
                td () { row.Dostepnosc }
                td () { row.Kategoria }
                td () { $"%i{row.LiczbaBeneficjentow}" }
                td () { row.OstatnieOdwiedzinyData |> toDisplay }
            }
    }
