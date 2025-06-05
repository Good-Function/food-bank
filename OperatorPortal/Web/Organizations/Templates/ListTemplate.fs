module Organizations.ListTemplate

open Layout
open Organizations.SearchableListTemplate
open Organizations.Templates.Formatters
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Organizations.Application.ReadModels
open Web.Layout.Dropdown

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
            th(style = "width: 120px; text-align:center;") {"Kontakt"}
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
                    td () { row.Kategoria }
                    td () { row.Beneficjenci }
                    td () { $"%i{row.LiczbaBeneficjentow}" }
                    td () { row.OstatnieOdwiedzinyData |> toDisplay }
                    td (style="text-align: center;") { DropDown (40, Icons.ContactPage) (Placement.Left, address(style="width:400px; display:flex; gap: 5px; margin-bottom:0;"){
                        div(style="display:flex; flex-direction: column; padding-right: 5px; border-right: 1px solid var(--pico-contrast); text-align:right;") {
                            b () { "Telefon" }
                            b () { "Email" }
                            b () { "Kontakt" }
                            b () { "Osoba" }
                            b () { "Tel. osoby" }
                            b () { "Dostępność" }            
                        }
                        div(style="display:flex; flex-direction: column; white-space:nowrap; overflow:auto;") {
                            span() { row.Telefon }
                            span() { row.Email }   
                            span() { row.Kontakt }   
                            span() { row.OsobaDoKontaktu }   
                            span() { row.TelefonOsobyKontaktowej }   
                            span() { row.Dostepnosc }   
                        }  
                    }) } 
                }
        }

        filterTemplate filter
    }
