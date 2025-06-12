module Organizations.List.SearchableListTemplate

open Layout
open Layout.Navigation
open Organizations.Application.ReadModels.OrganizationSummary
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations
open PageComposer
open Web.Organizations.Templates.List.Filterable
open Web.Organizations.Templates.List.Sortable

let createFilterStateHolder filter =
            match filter.SortBy with
            | None -> Fragment() {}
            | Some(sort, dir) ->
                Fragment() {
                    input (type' = "hidden", name = "sort", value = sort)
                    input (type' = "hidden", name = "dir", value = dir.ToString())
                }

let Template (query: Query) =
    div (id = "OrganizationsPage") {
        div(id = "OrganizationsFilterState") {
            createFilterStateHolder query
        }
        input (
            type' = "search",
            name = "search",
            value = query.SearchTerm,
            id = "OrganizationSearch",
            style = "transition:none;",
            title = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxGet = "/organizations/summaries",
            hxInclude = "[name='sort'], [name='dir'], [name='LiczbaBeneficjentow'], [name='LiczbaBeneficjentow_op']",
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
