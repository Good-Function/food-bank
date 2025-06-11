module Organizations.List.SearchableListTemplate

open Layout
open Layout.Navigation
open Organizations.Application.ReadModels.OrganizationSummary
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations
open PageComposer

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
                            th (style = "width:82px;") { "Teczk." }
                            th (style = "width:290px;") { "Nazwa placówki" }
                            th (style = "width:300px;") { "Adres placówki" }
                            th (style = "width:200px;") { "Gmina/Dzielnica" }
                            th (style = "width:175px;") { "Forma prawna" }
                            th (style = "width:200px;") { "Kategoria" }
                            th (style = "width:200px;") { "Beneficjenci" }
                            th (style = "width:155px;") { "Liczba B." }
                            th (style = "width:150px;") { "Odwiedzono" }
                            th (style = "width:110px;") { "Kontakt" }
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
