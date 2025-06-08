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

type FilterType =
    | StringFilter
    | NumberFilter

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

let buildQueryForSorting (column: string, filter: Query) : string =
    let queryParams = Map.empty

    let queryParams =
        match filter.SortBy with
        | Some(sort, dir) when sort = column ->
            queryParams |> Map.add "sort" sort |> Map.add "dir" (dir.Reverse().ToString())
        | _ -> queryParams |> Map.add "sort" column |> Map.add "dir" (Direction.Asc.ToString())
    QueryHelpers.AddQueryString("", queryParams)

let createSortableBy (columnName: string) (label: string) (filter: Query)=
    let url = $"""/organizations/summaries{buildQueryForSorting (columnName, filter)}"""
    div (style="display:flex;") {
        a (
            hxGet = url,
            href = url,
            hxTarget = "#OrganizationsList",
            hxTrigger = "click",
            hxPushUrl = "true",
            hxSwap = "outerHTML",
            hxInclude = """[name='liczba_beneficjentow'], [name='liczba_beneficjentow_op'], [name='search']""",
            hxIndicator = ".big-table",
            style = "color:unset;"
        ) {
            div () { label }
        }
        div (style = "text-decoration: none;") {
            match filter.SortBy with
            | Some(sort, dir) when sort = columnName && dir = Direction.Asc -> "▲"
            | Some(sort, _) when sort = columnName -> "▼"
            | _ -> ""
        }
    }
    
let createFilterableSortableBy (columnName: string) (labelText: string) query =
    let url = $"""/organizations/summaries{buildQueryForSorting (columnName, query)}"""
    let columnFilter = query.Filters |> List.tryFind(fun filter -> filter.Key = columnName)
    div (style="display:flex;justify-content:space-between;") {
        a (
            hxGet = url,
            href = url,
            hxTarget = "#OrganizationsList",
            hxTrigger = "click",
            hxPushUrl = "true",
            hxSwap = "outerHTML",
            hxInclude = """[name='liczba_beneficjentow'], [name='liczba_beneficjentow_op'], [name='search']""",
            hxIndicator = ".big-table",
            style = "color:unset;"
        ) {
            span (style="text-decoration: underline") { labelText }
            span (style = "text-decoration: none;") {
                match query.SortBy with
                | Some(sort, dir) when sort = columnName && dir = Direction.Asc -> "▲"
                | Some(sort, _) when sort =  columnName -> "▼"
                | _ -> ""
            }
        }
        let hasOp op = Option.exists (fun cf -> cf.Operator = op)  
        DropDown (24, Icons.FilterEmpty) (Placement.Bottom, div(style="width:300px;") {
            div(
                style="display:flex; align-items: baseline; justify-content: space-between"
                ) {
                span (style="white-space:no-break") { labelText }
                select (
                        name="liczba_beneficjentow_op",
                        style = "height: 34.5px; margin: 0; padding-right: 5px; padding-top: 0; padding-bottom: 0; width: 100px;"
                    ) {
                    option(selected = (columnFilter |> hasOp "=")) { "=" }
                    option(selected = (columnFilter |> hasOp ">")) { ">" }
                    option(selected = (columnFilter |> hasOp "<")) { "<" }
                    option(selected = (columnFilter |> hasOp ">=")) { ">=" }
                    option(selected = (columnFilter |> hasOp "<=")) { "<=" }
                }
                input (
                    hxTrigger = "keyup[key=='Enter']",
                    hxTarget = "#OrganizationsList",
                    hxSwap = "outerHTML",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = "[name='sort'], [name='dir'], [name='liczba_beneficjentow_op'], [name='search']",
                    style="width: 100px; margin-bottom:0;",
                    value = (columnFilter |> Option.map _.Value.ToString() |> Option.defaultValue ""),
                    name = "liczba_beneficjentow",
                    type'="number")
            }
            div(style="display: flex; justify-content:space-between; gap:var(--pico-spacing); margin-top:var(--pico-spacing); font-weight:normal"){
                div(
                    style="cursor:pointer",
                    hxTarget = "#OrganizationsList",
                    hxSwap = "outerHTML",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = "[name='sort'], [name='dir'], [name='search']"
                    ){
                    Icons.Cancel
                    "usuń"
                }
                div(
                    style="cursor:pointer",
                    hxTarget = "#OrganizationsList",
                    hxSwap = "outerHTML",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = "[name='sort'], [name='dir'], [name='liczba_beneficjentow'], [name='liczba_beneficjentow_op'], [name='search']"
                    ){
                    "zastosuj"
                    Icons.Ok
                }
            }
        })
    }
    
let createFilterStateHolder filter =
            match filter.SortBy with
            | None -> Fragment() {}
            | Some(sort, dir) ->
                Fragment() {
                    input (type' = "hidden", name = "sort", value = sort)
                    input (type' = "hidden", name = "dir", value = dir.ToString())
                }

let Template (currentFilter: Query) =
    div (id = "OrganizationsPage") {
        div(id = "OrganizationsFilterState") {
            createFilterStateHolder currentFilter
        }
        input (
            type' = "search",
            name = "search",
            value = currentFilter.SearchTerm,
            id = "OrganizationSearch",
            style = "transition:none;",
            title = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxGet = "/organizations/summaries",
            hxInclude = "[name='sort'], [name='dir'], [name='liczba_beneficjentow'], [name='liczba_beneficjentow_op']",
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

let FullPage (filter: Query) =
    composeFullPage
        { Content = Template filter
          CurrentPage = Page.Organizations }
