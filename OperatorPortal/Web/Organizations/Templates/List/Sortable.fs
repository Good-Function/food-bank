module Web.Organizations.Templates.List.Sortable

open Microsoft.AspNetCore.WebUtilities
open Organizations.Application.ReadModels.OrganizationSummary
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let private buildQueryForSorting (column: string, sortBy: (string * Direction) option) : string =
    let queryParams = Map.empty

    let queryParams =
        match sortBy with
        | Some(sort, dir) when sort = column ->
            queryParams |> Map.add "sort" sort |> Map.add "dir" (dir.Reverse().ToString())
        | _ -> queryParams |> Map.add "sort" column |> Map.add "dir" (Direction.Asc.ToString())
    QueryHelpers.AddQueryString("", queryParams)
    
type SortBy = {
    ColumnKey: string
    ColumnLabel: string
    CurrentSortBy: (string * Direction) option
}

let sortable (sortBy: SortBy) =
     div (style="display:inline-flex;") {
            let url = $"""/organizations/summaries{buildQueryForSorting (sortBy.ColumnKey, sortBy.CurrentSortBy)}"""
            a (
                hxGet = url,
                href = url,
                hxTarget = "#OrganizationsList",
                hxTrigger = "click",
                hxPushUrl = "true",
                hxSwap = "outerHTML",
                hxInclude = """[name='LiczbaBeneficjentow'], [name='LiczbaBeneficjentow_op'], [name='search']""",
                hxIndicator = ".big-table",
                style = "color:unset;"
            ) {
                div () { sortBy.ColumnLabel }
            }
            pre(style="margin:0; font-size:1.1rem; background:none") {
                match sortBy.CurrentSortBy with
                | Some(sort, dir) when sort = sortBy.ColumnKey && dir = Direction.Asc -> "▲"
                | Some(sort, _) when sort = sortBy.ColumnKey -> "▼"
                | _ -> " "
            }
         }