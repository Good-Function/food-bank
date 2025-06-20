module Organizations.List.SearchableListTemplate

open Layout
open Layout.Navigation
open Organizations.Application.ReadModels.OrganizationSummary
open Organizations.Templates.List
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Organizations
open PageComposer

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
            hxIndicator = ".big-table"
        )
        small (style="position:relative;") {
            div (style = "overflow-x: scroll; height: 66vh";) {
                table (class' = "striped big-table") {
                    thead () {
                        tr (id="OrganizationHeadersRow") {
                            TableHeader.build query
                        }
                    }
                    Indicators.TableShimmeringRows 6 9
                    tbody (id = "OrganizationsList") { }
                }
            }
            div (class' = "big-table-footer") {
                button (class' = "outline secondary", title = "Pobierz listę mailingową. (prace trwają).") {
                    small () {
                        div () { Icons.Mail }
                        "Lista mailingowa"
                    }
                }
                div(id="OrganizationsTablePagination") {
                    Pagination.build query.Pagination None
                }
            }
            div(class'="empty-table-message") {
                span(id="OrganizationsEmptyTableMessage"){}
            }
        }
    }

let FullPage (filter: Query) =
    composeFullPage
        { Content = Template filter
          CurrentPage = Page.Organizations }
