module Organizations.ListTemplate

open Organizations.SearchableListTemplate
open Organizations.Templates.Formatters
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Organizations.Application.ReadModels

let filterTemplate filter =
    Fragment() {
        div(id = "OrganizationsFilterState", hxSwapOob = "true") {
            createFilterStateHolder filter
        }
        input (
            hxSwapOob = "true",
            type' = "search",
            name = "search",
            value = filter.searchTerm,
            id = "OrganizationSearch",
            style = "transition:none;",
            title = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxGet = "/organizations/summaries",
            hxInclude = "[name='sort'], [name='dir'], [name='liczba_beneficjentow_gt'], [name='liczba_beneficjentow_lt']",
            placeholder = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxTrigger = "input changed delay:500ms, keyup[key=='Enter']",
            hxSync = "this:replace",
            hxSwap = "outerHTML",
            hxIndicator = ".big-table",
            hxTarget = "#OrganizationsList",
            hxPushUrl = "true"
        )
        tr (id="OrganizationHeadersRow", hxSwapOob = "true") {
            for column in Columns do
                th (style = $"width:{column.Width}px;") {
                    filter |> createSortableBy column.Name column.Label
                }
        }
    }


let Template (data: OrganizationSummary list) (filter: Filter) =
    Fragment() {
        tbody (id = "OrganizationsList", class'="hide-on-request") {
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
                    // td () { row.Telefon }
                    // td () { row.Email }
                    // td () { row.Kontakt }
                    // td () { row.OsobaDoKontaktu }
                    // td () { row.TelefonOsobyKontaktowej }
                    // td () { row.Dostepnosc }
                    td () { row.Kategoria }
                    td () { row.Beneficjenci }
                    td () { $"%i{row.LiczbaBeneficjentow}" }
                    td () { row.OstatnieOdwiedzinyData |> toDisplay }
                }
        }

        filterTemplate filter
    }
