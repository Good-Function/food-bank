module Organizations.SearchableListTemplate

open Layout
open Layout.Navigation
open Microsoft.AspNetCore.WebUtilities
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Layout.Dropdown
open Web.Organizations
open PageComposer

type Column = {
    Label: string
    Name: string
    Width: int
}
let Columns: Column list = [
    { Name = "Teczka"; Label = "Teczk."; Width = 82 }
    { Name = "NazwaPlacowkiTrafiaZywnosc"; Label = "Nazwa placówki"; Width = 290 }
    { Name = "AdresPlacowkiTrafiaZywnosc"; Label = "Adres placówki"; Width = 300 }
    { Name = "GminaDzielnica"; Label = "Gmina/Dzielnica"; Width = 200 }
    { Name = "FormaPrawna"; Label = "Forma prawna"; Width = 175 }
    { Name = "Kategoria"; Label = "Kategoria"; Width = 200 }
    { Name = "Beneficjenci"; Label = "Beneficjenci"; Width = 200 }
    { Name = "LiczbaBeneficjentow"; Label = "Liczba B."; Width = 155 }
    { Name = "OstatnieOdwiedzinyData"; Label = "Odwiedzono"; Width = 150 }
]

let buildQueryForSorting (column: string, filter: Filter) : string =
    let queryParams = Map.empty

    let queryParams =
        match filter.sortBy with
        | Some(sort, dir) when sort = column ->
            queryParams |> Map.add "sort" sort |> Map.add "dir" (dir.Reverse().ToString())
        | _ -> queryParams |> Map.add "sort" column |> Map.add "dir" (Direction.Asc.ToString())

    QueryHelpers.AddQueryString("", queryParams)

let createSortableBy (columnName: string) (label: string) (filter: Filter)=
    let url = $"""/organizations/summaries{buildQueryForSorting (columnName, filter)}"""
    div (style="display:flex;") {
        a (
            hxGet = url,
            href = url,
            hxTarget = "#OrganizationsList",
            hxTrigger = "click",
            hxPushUrl = "true",
            hxSwap = "outerHTML",
            hxInclude = """[name='liczba_beneficjentow_gt'], [name='liczba_beneficjentow_lt'], [name='search']""",
            hxIndicator = ".big-table",
            style = "color:unset;"
        ) {
            div () { label }
        }
        div (style = "text-decoration: none;") {
            match filter.sortBy with
            | Some(sort, dir) when sort = columnName && dir = Direction.Asc -> "▲"
            | Some(sort, _) when sort = columnName -> "▼"
            | _ -> ""
        }
    }
    
let createFilterableSortableBy (labelText: string) filter =
    let url = $"""/organizations/list{buildQueryForSorting ("LiczbaBeneficjentow", filter)}"""
    div (style="display:flex;justify-content:space-between;") {
        a (
            hxGet = url,
            href = url,
            hxTarget = "#OrganizationsPage",
            hxTrigger = "click",
            hxPushUrl = "true",
            hxInclude = """[name='liczba_beneficjentow_gt'], [name='liczba_beneficjentow_lt'], [name='search']""",
            hxIndicator = ".big-table",
            style = "color:unset; text-decoration: none"
        ) {
            span (style="text-decoration: underline") { labelText }
            span (style = "text-decoration: none;") {
                match filter.sortBy with
                | Some(sort, dir) when sort = "LiczbaBeneficjentow" && dir = Direction.Asc -> "▲"
                | Some(sort, _) when sort =  "LiczbaBeneficjentow" -> "▼"
                | _ -> ""
            }
        }
        DropDown 24 Icons.FilterEmpty (div(style="width:165px;") {
            InProgress.Component
            label () { "Od" }
            input (
                value = (filter.beneficjenci.gt |> Option.map _.ToString() |> Option.defaultValue("")),
                name = "liczba_beneficjentow_gt",
                type'="number")
            label () { "Do" }
            input (
                value = (filter.beneficjenci.lt |> Option.map _.ToString() |> Option.defaultValue("")),
                name = "liczba_beneficjentow_lt",
                type'="number")
        })
    }
    
let createFilterStateHolder filter =
            match filter.sortBy with
            | None -> Fragment() {}
            | Some(sort, dir) ->
                Fragment() {
                    input (type' = "hidden", name = "sort", value = sort)
                    input (type' = "hidden", name = "dir", value = dir.ToString())
                }

let Template (filter: Filter) =
    div (id = "OrganizationsPage") {
        div(id = "OrganizationsFilterState") {
            createFilterStateHolder filter
        }
        input (
            type' = "search",
            name = "search",
            value = filter.searchTerm,
            id = "OrganizationSearch",
            style = "transition:none;",
            title = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxGet = "/organizations/summaries",
            hxInclude = "[name='sort'], [name='dir'], [name='liczba_beneficjentow_gt'], [name='liczba_beneficjentow_lt']",
            placeholder = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxTrigger = "load, input changed delay:500ms, keyup[key=='Enter']",
            hxSync = "this:replace",
            hxSwap = "outerHTML",
            hxTarget = "#OrganizationsList",
            hxIndicator = ".big-table",
            hxPushUrl = "true"
        )
        small () {
            div (style = "overflow-x: scroll; height: 70vh;";) {
                table (class' = "striped big-table") {
                    thead () {
                        tr (id="OrganizationHeadersRow") {
                            for column in Columns do
                                th (style = $"width:{column.Width}px;") {
                                    filter |> createSortableBy column.Name column.Label
                                }
                            // th (style = "width: 200px;") { "Telefon" }
                            // th (style = "width: 200px;") { "Email" }
                            // th (style = "width: 200px;") { "Kontakt" }
                            // th (style = "width: 200px;") { "Osoba" }
                            // th (style = "width: 200px;") { "Tel. osoby" }
                            // th (style = "width: 200px;") { "Dostępność" }
                        }
                    }
                    Indicators.TableShimmeringRows 6 9
                    tbody (id = "OrganizationsList") { }
                }
            }
        }
    }

let FullPage (filter: Filter) =
    composeFullPage
        { Content = Template filter
          CurrentPage = Page.Organizations }
