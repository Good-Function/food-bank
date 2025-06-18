module Organizations.Templates.List.Sortable

open Microsoft.AspNetCore.WebUtilities
open Organizations.Application.ReadModels
open Organizations.Application.ReadModels.OrganizationSummary
open Organizations.Application.ReadModels.QueriedColumn
open Oxpecker.ViewEngine
open Oxpecker.Htmx

type SortBy = {
    Column: QueriedColumn
    CurrentSortBy: (QueriedColumn * Direction) option
}

let private buildQueryForSorting (sortBy: SortBy) : string =
    let queryParams = Map.empty

    let queryParams =
        match sortBy.CurrentSortBy with
        | Some(sort, dir) when sort = sortBy.Column ->
            queryParams |> Map.add "sort" $"{sort}" |> Map.add "dir" (dir.Reverse().ToString())
        | _ -> queryParams |> Map.add "sort" $"{sortBy.Column}" |> Map.add "dir" (Direction.Asc.ToString())
    QueryHelpers.AddQueryString("", queryParams)

let sortable (sortBy: SortBy) =
     div (style="display:inline-flex;") {
            let url = $"""/organizations/summaries{buildQueryForSorting sortBy}"""
            a (
                hxGet = url,
                href = url,
                hxTarget = "#OrganizationsList",
                hxTrigger = "click",
                hxPushUrl = "true",
                hxInclude = $"""[name='search'], {HxIncludes.all}""",
                hxIndicator = ".big-table",
                style = "color:unset;"
            ) {
                div () { sortBy.Column.Label }
            }
            pre(style="margin:0; font-size:1.1rem; background:none") {
                match sortBy.CurrentSortBy with
                | Some(sort, dir) when sort = sortBy.Column && dir = Direction.Asc -> "▲"
                | Some(sort, _) when sort = sortBy.Column -> "▼"
                | _ -> " "
            }
         }