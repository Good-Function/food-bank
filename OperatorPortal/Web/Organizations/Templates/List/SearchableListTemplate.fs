module Organizations.List.SearchableListTemplate

open Layout
open Layout.Navigation
open Organizations.Application.ReadModels.QueriedColumn
open Organizations.Application.ReadModels.OrganizationSummary
open Organizations.Templates.List
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations
open PageComposer
open Organizations.Templates.List.Filterable
open Organizations.Templates.List.Sortable

let createFilterStateHolder filter =
            match filter.SortBy with
            | None -> Fragment() {}
            | Some(sort, dir) ->
                Fragment() {
                    input (type' = "hidden", name = "sort", value = $"{sort}")
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
            hxInclude = $"[name='sort'], [name='dir'], {HxIncludes.all}",
            placeholder = "Szukaj po teczce, nazwie placówki, gminie/dzielnicy.",
            hxTrigger = "load, input changed delay:500ms, keyup[key=='Enter']",
            hxSync = "this:replace",
            hxTarget = "#OrganizationsList",
            hxIndicator = ".big-table",
            hxPushUrl = "true"
        )
        small () {
            div (style = "overflow-x: scroll; height: 70vh;";) {
                table (class' = "striped big-table") {
                    thead () {
                        tr (id="OrganizationHeadersRow") {
                            th (style = "width:85px;") {
                                sortable {
                                    Column = Teczka
                                    CurrentSortBy = query.SortBy
                                }
                            }
                            th (style = "width:290px;") {
                                sortable {
                                    Column = NazwaPlacowkiTrafiaZywnosc
                                    CurrentSortBy = query.SortBy
                                }
                                filterable {
                                    Type = FilterType.StringFilter
                                    Column = NazwaPlacowkiTrafiaZywnosc
                                    CurrentFilters = query.Filters
                                }
                            }
                            th (style = "width:300px;") {
                                sortable {
                                    Column = AdresPlacowkiTrafiaZywnosc
                                    CurrentSortBy = query.SortBy
                                }
                                filterable {
                                    Type = FilterType.StringFilter
                                    Column = AdresPlacowkiTrafiaZywnosc
                                    CurrentFilters = query.Filters
                                }
                            }
                            th (style = "width:220px;") {
                                sortable {
                                    Column = GminaDzielnica
                                    CurrentSortBy = query.SortBy
                                }
                                filterable {
                                    Type = FilterType.StringFilter
                                    Column = GminaDzielnica
                                    CurrentFilters = query.Filters
                                }
                            }
                            th (style = "width:200px;") {
                                sortable {
                                    Column = FormaPrawna
                                    CurrentSortBy = query.SortBy
                                }
                                filterable {
                                    Type = FilterType.StringFilter
                                    Column = FormaPrawna
                                    CurrentFilters = query.Filters
                                }
                            }
                            th (style = "width:200px;") {
                                sortable {
                                    Column = Kategoria
                                    CurrentSortBy = query.SortBy
                                }
                                filterable {
                                    Type = FilterType.StringFilter
                                    Column = Kategoria
                                    CurrentFilters = query.Filters
                                }
                            }
                            th (style = "width:200px;") {
                                sortable {
                                    Column = Beneficjenci
                                    CurrentSortBy = query.SortBy
                                }
                                filterable {
                                    Type = FilterType.StringFilter
                                    Column = Beneficjenci
                                    CurrentFilters = query.Filters
                                }
                            }
                            th (style = "width:155px;") {
                                sortable {
                                    Column = LiczbaBeneficjentow
                                    CurrentSortBy = query.SortBy
                                }
                                filterable {
                                    Type = FilterType.NumberFilter
                                    Column = LiczbaBeneficjentow
                                    CurrentFilters = query.Filters
                                }
                            }
                            th (style = "width:150px;") {
                                sortable {
                                    Column = OstatnieOdwiedzinyData
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
