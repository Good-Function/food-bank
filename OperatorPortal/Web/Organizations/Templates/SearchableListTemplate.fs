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

type StringOperator =
    | Equals
    | Contains

type NumberOperator =
    | Equals
    | GreaterThan
    | LessThan
    | GreaterThanOrEqual
    | LessThanOrEqual
    with override this.ToString() =
            match this with
            | Equals -> "="
            | GreaterThan -> ">"
            | LessThan -> failwith "<"
            | GreaterThanOrEqual -> failwith ">="
            | LessThanOrEqual -> failwith "<="
         static member Parse operator =
             match operator with
             | ">"  -> GreaterThan
             | "<"  -> LessThan
             | ">=" -> GreaterThanOrEqual
             | "<=" -> LessThanOrEqual
             | _ -> Equals

type FilterType =
    | StringFilter // of StringOperator * string
    | NumberFilter // of NumberOperator * int

type SortAndFilter = {
    Key: string
    Filter: FilterType option
}

type Column = {
    Label: string
    Width: int
    SortAndFilter: SortAndFilter option
}

let Columns: Column list = [
    { Label = "Teczk."; Width = 82; SortAndFilter = Some { Key="Teczka"; Filter = None } }
    { Label = "Nazwa placówki"; Width = 290; SortAndFilter = Some { Key="NazwaPlacowkiTrafiaZywnosc"; Filter = None } }
    { Label = "Adres placówki"; Width = 300; SortAndFilter = Some { Key="AdresPlacowkiTrafiaZywnosc"; Filter = None } }
    { Label = "Gmina/Dzielnica"; Width = 200; SortAndFilter = Some { Key="GminaDzielnica"; Filter = None } }
    { Label = "Forma prawna"; Width = 175; SortAndFilter = Some { Key="FormaPrawna"; Filter = None } }
    { Label = "Kategoria"; Width = 200; SortAndFilter = Some { Key="Kategoria"; Filter = None } }
    { Label = "Beneficjenci"; Width = 200; SortAndFilter = Some { Key="Beneficjenci"; Filter = None } }
    { Label = "Liczba B."; Width = 155; SortAndFilter = Some { Key="LiczbaBeneficjentow"; Filter = Some NumberFilter } }
    { Label = "Odwiedzono"; Width = 150; SortAndFilter = Some { Key="OstatnieOdwiedzinyData"; Filter = None } }
    { Label = "Kontakt"; Width = 110; SortAndFilter = None }
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
    
let createFilterableSortableBy (columnName: string) (labelText: string) (filterType: FilterType) filter =
    let url = $"""/organizations/summaries{buildQueryForSorting (columnName, filter)}"""
    div (style="display:flex;justify-content:space-between;") {
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
            span (style="text-decoration: underline") { labelText }
            span (style = "text-decoration: none;") {
                match filter.sortBy with
                | Some(sort, dir) when sort = columnName && dir = Direction.Asc -> "▲"
                | Some(sort, _) when sort =  columnName -> "▼"
                | _ -> ""
            }
        }
        DropDown (24, Icons.FilterEmpty) (Placement.Bottom, div(style="width:300px;") {
            InProgress.Component
            div(style="display:flex; align-items: baseline; justify-content: space-between") {
                span (style="white-space:no-break") { labelText }
                select (style="height: 34.5px; margin: 0; padding-right: 5px; padding-top: 0; padding-bottom: 0; width: 100px;") {
                    option() { "=" }
                    option() { ">" }
                    option() { "<" }
                    option() { ">=" }
                    option() { "<=" }
                } 
                input (
                    style="width: 100px; margin-bottom:0;",
                    value = (filter.beneficjenci.lt |> Option.map _.ToString() |> Option.defaultValue("")),
                    name = "liczba_beneficjentow_lt",
                    type'="number")
            }
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

let Template (currentFilter: Filter) =
    div (id = "OrganizationsPage") {
        div(id = "OrganizationsFilterState") {
            createFilterStateHolder currentFilter
        }
        input (
            type' = "search",
            name = "search",
            value = currentFilter.searchTerm,
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
                                    column.Label
                                }
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
