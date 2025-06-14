module Organizations.List.ListTemplate

open Layout
open Organizations.List.SearchableListTemplate
open Organizations.Templates.Formatters
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Organizations.Application.ReadModels.OrganizationSummary
open Web.Layout.Dropdown
open Web.Organizations.Templates.List.Filterable
open Web.Organizations.Templates.List.Sortable

let filterTemplate query =
    Fragment() {
        div(id = "OrganizationsFilterState", hxSwapOob = "true") {
            createFilterStateHolder query
        }
        input (
            hxSwapOob = "true",
            type' = "search",
            name = "search",
            value = query.SearchTerm,
            id = "OrganizationSearch",
            style = "transition:none;",
            title = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxGet = "/organizations/summaries",
            hxInclude = "[name='sort'], [name='dir'], [name='LiczbaBeneficjentow'], [name='LiczbaBeneficjentow_op']",
            placeholder = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxTrigger = "input changed delay:500ms, keyup[key=='Enter']",
            hxSync = "this:replace",
            hxIndicator = ".big-table",
            hxTarget = "#OrganizationsList",
            hxPushUrl = "true"
        )

        tr (id="OrganizationHeadersRow", hxSwapOob = "true") {
            th (style = "width:82px;") {
                sortable {
                    ColumnKey = "Teczka"
                    ColumnLabel = "Teczk."
                    CurrentSortBy = query.SortBy
                }
            }
            th (style = "width:290px;") {
                sortable {
                    ColumnKey = "NazwaPlacowkiTrafiaZywnosc"
                    ColumnLabel = "Nazwa placówki"
                    CurrentSortBy = query.SortBy
                }
            }
            th (style = "width:300px;") {
                sortable {
                    ColumnKey = "AdresPlacowkiTrafiaZywnosc"
                    ColumnLabel = "Adres placówki"
                    CurrentSortBy = query.SortBy
                }
            }
            th (style = "width:200px;") {
                sortable {
                    ColumnKey = "GminaDzielnica"
                    ColumnLabel = "Gmina/Dzielnica"
                    CurrentSortBy = query.SortBy
                }
            }
            th (style = "width:175px;") {
                sortable {
                    ColumnKey = "FormaPrawna"
                    ColumnLabel = "Forma prawna"
                    CurrentSortBy = query.SortBy
                }
            }
            th (style = "width:200px;") {
                sortable {
                    ColumnKey = "Kategoria"
                    ColumnLabel = "Forma prawna"
                    CurrentSortBy = query.SortBy
                }
            }
            th (style = "width:200px;") {
                sortable {
                    ColumnKey = "Beneficjenci"
                    ColumnLabel = "Beneficjenci"
                    CurrentSortBy = query.SortBy
                }
                filterable {
                    Type = FilterType.StringFilter
                    ColumnKey = "Beneficjenci"
                    FilterLabel = "Beneficjenci"
                    CurrentFilters = query.Filters
                }
            }
            th (style = "width:155px;") {
                sortable {
                    ColumnKey = "LiczbaBeneficjentow"
                    ColumnLabel = "Liczba B."
                    CurrentSortBy = query.SortBy
                }
                filterable {
                    Type = FilterType.NumberFilter
                    ColumnKey = "LiczbaBeneficjentow"
                    FilterLabel = "Liczba B."
                    CurrentFilters = query.Filters
                }
            }
            th (style = "width:150px;") {
                sortable {
                    ColumnKey = "OstatnieOdwiedzinyData"
                    ColumnLabel = "Odwiedzono"
                    CurrentSortBy = query.SortBy
                }
            }
            th (style = "width:110px;") { "Kontakt" }
        }
    }


let Template (data: OrganizationSummary list) (filter: Query) =
    Fragment() {
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

        filterTemplate filter
    }
