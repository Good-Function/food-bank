module Organizations.ListTemplate

open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Organizations.Application.ReadModels
open Organizations.Funcs

let Template (data: OrganizationSummary list) =
    table (class' = "striped") {
        thead () {
            tr () {
                th () { "Teczka" }
                th () { "Nazwa placówki" }
                th (style = "min-width:200px;") { "Adres placówki" }
                th () { "Gmina/Dzielnica" }
                th () { "Forma prawna" }
                th (style = "min-width:150px") { "Telefon" }
                th () { "Email" }
                th (style = "min-width:200px") { "Kontakt" }
                th () { "Osoba do kontaktu" }
                th (style = "min-width:180px") { "Telefon do os. kontaktowej" }
                th () { "Dostępność" }
                th () { "Kateogria" }
                th () { "Liczba Beneficjentów" }

                th () {
                    div (
                        hxVals = """{"orderBy": "OstatnieOdwiedzinyData"}""",
                        hxGet = "/organizations/summaries",
                        hxTrigger = "click",
                        hxTarget = "#OrganizationsList",
                        hxInclude = "[name='search'], [name='orderBy']",
                        hxPushUrl = "true" // I think we need here hxPushUrl = "/organizations/list?search=lala&orderBy=OstatnieOdwiedzinyData" and pass search down here to render proper href.
                    ) {
                        "Ostanie odwiedziny"
                    }
                }
            }
        }

        tbody () {
            for row in data do
                tr (style = "position: relative") {
                    td () {
                        a (href = $"/organizations/%i{row.Teczka}", hxTarget = "#OrganizationsPage") {
                            $"%i{row.Teczka}"
                        }
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
                    td () { row.OstatnieOdwiedzinyData |> formatDate }
                }
        }
    }

