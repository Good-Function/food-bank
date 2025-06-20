module Organizations.Templates.List.TableHeaderBuilder

open Organizations.Application.ReadModels.QueriedColumn
open Organizations.Application.ReadModels.OrganizationSummary
open Oxpecker.ViewEngine
open Filterable
open Sortable

let build (query: Query) =
    Fragment() {
        th (style = "width:85px;") {
            sortable
                { Column = Teczka
                  CurrentSortBy = query.SortBy }
        }

        th (style = "width:290px;") {
            sortable
                { Column = NazwaPlacowkiTrafiaZywnosc
                  CurrentSortBy = query.SortBy }

            filterable
                { Type = FilterType.StringFilter
                  Column = NazwaPlacowkiTrafiaZywnosc
                  CurrentFilters = query.Filters }
        }

        th (style = "width:300px;") {
            sortable
                { Column = AdresPlacowkiTrafiaZywnosc
                  CurrentSortBy = query.SortBy }

            filterable
                { Type = FilterType.StringFilter
                  Column = AdresPlacowkiTrafiaZywnosc
                  CurrentFilters = query.Filters }
        }

        th (style = "width:220px;") {
            sortable
                { Column = GminaDzielnica
                  CurrentSortBy = query.SortBy }

            filterable
                { Type = FilterType.StringFilter
                  Column = GminaDzielnica
                  CurrentFilters = query.Filters }
        }

        th (style = "width:200px;") {
            sortable
                { Column = FormaPrawna
                  CurrentSortBy = query.SortBy }

            filterable
                { Type = FilterType.StringFilter
                  Column = FormaPrawna
                  CurrentFilters = query.Filters }
        }

        th (style = "width:200px;") {
            sortable
                { Column = Kategoria
                  CurrentSortBy = query.SortBy }

            filterable
                { Type = FilterType.StringFilter
                  Column = Kategoria
                  CurrentFilters = query.Filters }
        }

        th (style = "width:200px;") {
            sortable
                { Column = Beneficjenci
                  CurrentSortBy = query.SortBy }

            filterable
                { Type = FilterType.StringFilter
                  Column = Beneficjenci
                  CurrentFilters = query.Filters }
        }

        th (style = "width:155px;") {
            sortable
                { Column = LiczbaBeneficjentow
                  CurrentSortBy = query.SortBy }

            filterable
                { Type = FilterType.NumberFilter
                  Column = LiczbaBeneficjentow
                  CurrentFilters = query.Filters }
        }

        th (style = "width:150px;") {
            sortable
                { Column = OstatnieOdwiedzinyData
                  CurrentSortBy = query.SortBy }
        }

        th (style = "width:110px;") { "Kontakt" }
    }
