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

let buildQueryForSorting (column: string, filter: Filter) : string =
    let queryParams = Map.empty

    let queryParams =
        match filter.sortBy with
        | Some(sort, dir) when sort = column ->
            queryParams |> Map.add "sort" sort |> Map.add "dir" (dir.Reverse().ToString())
        | _ -> queryParams |> Map.add "sort" column |> Map.add "dir" (Direction.Asc.ToString())

    QueryHelpers.AddQueryString("", queryParams)

let Template (filter: Filter) =
    let createSortableBy (columnName: string) (label: string) =
        let url = $"""/organizations/list{buildQueryForSorting (columnName, filter)}"""
        div (style="display:flex;") {
            a (
                hxGet = url,
                href = url,
                hxTarget = "#OrganizationsPage",
                hxTrigger = "click",
                hxPushUrl = "true",
                hxInclude = """[name='liczba_beneficjentow_gt'], [name='liczba_beneficjentow_lt'], [name='search']""",
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
    let createFilterableSortableBy (labelText: string) =
        let url = $"""/organizations/list{buildQueryForSorting ("LiczbaBeneficjentow", filter)}"""
        div (style="display:flex;justify-content:space-between;") {
            a (
                hxGet = url,
                href = url,
                hxTarget = "#OrganizationsPage",
                hxTrigger = "click",
                hxPushUrl = "true",
                hxInclude = """[name='liczba_beneficjentow_gt'], [name='liczba_beneficjentow_lt'], [name='search']""",
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
        
    div (id = "OrganizationsPage") {
        match filter.sortBy with
            | None -> ()
            | Some(sort, dir) ->
                input (type' = "hidden", name = "sort", value = sort)
                input (type' = "hidden", name = "dir", value = dir.ToString())
        input (
            type' = "search",
            name = "search",
            value = filter.searchTerm,
            id = "OrganizationSearch",
            style = "transition:none;",
            title = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxGet = "/organizations/list",
            hxInclude = "[name='sort'], [name='dir']",
            placeholder = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxTrigger = "input changed delay:300ms, keyup[key=='Enter']",
            hxTarget = "#OrganizationsPage",
            hxPushUrl = "true"
        )

        small () {
            div (style = "overflow-x: scroll; height: 70vh;";) {
                table (class' = "striped big-table") {
                    thead () {
                        tr () {
                            th (style = "width: 82px;") {
                                "Tecz." |> createSortableBy "Teczka"
                            }
                            th (style = "width: 290px;") { "Nazwa placówki" }
                            th (style = "width: 300px;") { "Adres placówki" }
                            th (style = "width: 200px;") {
                                "Gmina/Dzielnica" |> createSortableBy "GminaDzielnica"
                            }
                            th (style = "width: 175px;") {
                                "Forma prawna" |> createSortableBy "FormaPrawna"
                            }
                            th (style = "width: 200px;") { "Telefon" }
                            th (style = "width: 200px;") { "Email" }
                            th (style = "width: 200px;") { "Kontakt" }
                            th (style = "width: 200px;") { "Osoba" }
                            th (style = "width: 200px;") { "Tel. osoby" }
                            th (style = "width: 200px;") { "Dostępność" }
                            th (style = "width: 200px;") {
                                "Kategoria" |> createSortableBy "Kategoria"
                            }
                            th (style = "width: 200px;") {
                                "Beneficjenci" |> createSortableBy "Beneficjenci"
                            }
                            th (style = "width: 155px;") {
                                "Liczba B." |> createFilterableSortableBy
                            }
                            th (style = "width: 150px;") {
                                "Odwiedzono" |> createSortableBy "OstatnieOdwiedzinyData"
                            }
                        }
                    }

                    tbody (id = "OrganizationsList") {
                        for _ in 1..4 do
                            tr () { td (colspan = 14) { div (class' = "shimmer", style = "padding-bottom:3px;") } }

                        div (
                            hxGet = "/organizations/summaries",
                            hxTrigger = "load",
                            hxTarget = "#OrganizationsList",
                            hxSwap = "outerHTML",
                            hxInclude = "[name='search'], [name='sort'], [name='dir']"
                        )
                    }
                }
            }
        }
    }

let FullPage (filter: Filter) =
    composeFullPage
        { Content = Template filter
          CurrentPage = Page.Organizations }
