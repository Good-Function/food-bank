module Organizations.List.ListTemplate

open System
open Layout
open Organizations.List.SearchableListTemplate
open Organizations.Templates.Formatters
open Organizations.Templates.List
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Organizations.Application.ReadModels.OrganizationSummary
open Web.Layout.Dropdown

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
            hxInclude = $"[name='sort'], [name='dir'], {HxIncludes.all}",
            placeholder = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxTrigger = "input changed delay:500ms, keyup[key=='Enter']",
            hxSync = "this:replace",
            hxIndicator = ".big-table",
            hxTarget = "#OrganizationsList",
            hxPushUrl = "true"
        )
        tr (id="OrganizationHeadersRow", hxSwapOob = "true") {
            TableHeader.build query
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
                        div() { if String.IsNullOrWhiteSpace row.Telefon then "-" else row.Telefon}
                        div() { if String.IsNullOrWhiteSpace row.Email then "-" else row.Email}
                        div() { if String.IsNullOrWhiteSpace row.Kontakt then "-" else row.Kontakt}
                        div() { if String.IsNullOrWhiteSpace row.OsobaDoKontaktu then "-" else row.OsobaDoKontaktu}
                        div() { if String.IsNullOrWhiteSpace row.TelefonOsobyKontaktowej then "-" else row.TelefonOsobyKontaktowej}
                        div() { if String.IsNullOrWhiteSpace row.Dostepnosc then "-" else row.Dostepnosc}
                    }  
                }) } 
            }
        tr(){ }
        if data.Length = 0
            then span(hxSwapOob="true", id="OrganizationsEmptyTableMessage") { "Nie znaleziono danych." }
            else span(hxSwapOob="true", id="OrganizationsEmptyTableMessage") {}
        filterTemplate filter
        div (id="OrganizationsTablePagination", hxSwapOob="true") {
            Pagination.build filter.Pagination (Some data.Length)
        }
    }
