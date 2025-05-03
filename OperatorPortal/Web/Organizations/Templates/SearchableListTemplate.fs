module Organizations.SearchableListTemplate

open Layout.Navigation
open Microsoft.AspNetCore.WebUtilities
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations
open PageComposer

let buildQueryForSearch (filter: Filter) : string =
    let queryParams = Map.empty

    let queryParams =
        match filter.sortBy with
        | Some(sort, dir) -> queryParams |> Map.add "sort" sort |> Map.add "dir" (dir.ToString())
        | _ -> queryParams

    QueryHelpers.AddQueryString("", queryParams)

let buildQueryForSorting (column: string, filter: Filter) : string =
    let queryParams = Map.empty |> Map.add "search" filter.searchTerm

    let queryParams =
        match filter.sortBy with
        | Some(sort, dir) when sort = column ->
            queryParams |> Map.add "sort" sort |> Map.add "dir" (dir.Reverse().ToString())
        | _ -> queryParams |> Map.add "sort" column |> Map.add "dir" (Direction.Asc.ToString())

    QueryHelpers.AddQueryString("", queryParams)

let Template (filter: Filter) =
    div (id = "OrganizationsPage") {
        input (
            type' = "search",
            name = "search",
            value = filter.searchTerm,
            id = "OrganizationSearch",
            style = "transition:none;",
            title = "Szukaj po: Teczka, Nazwa placówki",
            hxGet = $"/organizations/list{buildQueryForSearch (filter)}",
            placeholder = "Szukaj po teczce, nazwie placówki...",
            hxTrigger = "input changed delay:300ms, keyup[key=='Enter']",
            hxTarget = "#OrganizationsPage",
            hxPushUrl = "true"
        )

        match filter.sortBy with
        | None -> ()
        | Some(sort, dir) ->
            input (type' = "hidden", name = "sort", value = sort)
            input (type' = "hidden", name = "dir", value = dir.ToString())

        small () {
            div (style = "overflow-x: scroll; max-height: 70vh") {
                table (class' = "striped", style = "table-layout:fixed") {
                    thead () {
                        tr () {
                            th (style = "width: 80px;") {
                                div (style="display:flex;") {
                                    a (
                                        hxGet = $"""/organizations/list{buildQueryForSorting ("Teczka", filter)}""",
                                        href = $"""/organizations/list{buildQueryForSorting ("Teczka", filter)}""",
                                        hxTarget = "#OrganizationsPage",
                                        hxTrigger = "click",
                                        hxPushUrl = "true",
                                        style = "color:unset;"
                                    ) {
                                        div () { "Tecz." }
                                    }
                                    div (style = "text-decoration: none;") {
                                        match filter.sortBy with
                                        | Some(sort, dir) when sort = "Teczka" && dir = Direction.Asc -> "▲"
                                        | Some(sort, _) when sort = "Teczka" -> "▼"
                                        | _ -> ""
                                    }
                                }
                            }

                            th (style = "width: 200px;") { "Nazwa placówki" }
                            th (style = "width: 300px;") { "Adres placówki" }
                            th (style = "width: 200px;") { "Gmina/Dzielnica" }
                            th (style = "width: 150px;") { "Forma prawna" }
                            th (style = "width: 200px;") { "Telefon" }
                            th (style = "width: 200px;") { "Email" }
                            th (style = "width: 200px;") { "Kontakt" }
                            th (style = "width: 200px;") { "Osoba do kontaktu" }
                            th (style = "width: 200px;") { "Tel. osoby kontaktowej" }
                            th (style = "width: 200px;") { "Dostępność" }
                            th (style = "width: 200px;") { "Kateogria" }
                            th (style = "width: 155px;") { "Liczba Beneficjentów" }

                            th (style = "width: 150px;") {
                                div (style = "display:flex; ") {
                                    a (
                                        hxGet =
                                            $"""/organizations/list{buildQueryForSorting ("OstatnieOdwiedzinyData", filter)}""",
                                        href =
                                            $"""/organizations/list{buildQueryForSorting ("OstatnieOdwiedzinyData", filter)}""",
                                        hxTarget = "#OrganizationsPage",
                                        hxTrigger = "click",
                                        hxPushUrl = "true",
                                        style = "color:unset;"
                                    ) {
                                        div () { "Ostatnie odwiedziny" }
                                    }
                                    div (style = "text-decoration: none;--pico-text-decoration: none;") {
                                        match filter.sortBy with
                                        | Some(sort, dir) when sort = "OstatnieOdwiedzinyData" && dir = Direction.Asc ->
                                            "▲"
                                        | Some(sort, _) when sort = "OstatnieOdwiedzinyData" -> "▼"
                                        | _ -> ""
                                    }
                                }
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
