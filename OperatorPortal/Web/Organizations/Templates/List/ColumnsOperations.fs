module Web.Organizations.Templates.List.ColumnsOperations

open Layout
open Microsoft.AspNetCore.WebUtilities
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Layout.Dropdown

let private buildQueryForSorting (column: string, filter: Query) : string =
    let queryParams = Map.empty

    let queryParams =
        match filter.SortBy with
        | Some(sort, dir) when sort = column ->
            queryParams |> Map.add "sort" sort |> Map.add "dir" (dir.Reverse().ToString())
        | _ -> queryParams |> Map.add "sort" column |> Map.add "dir" (Direction.Asc.ToString())
    QueryHelpers.AddQueryString("", queryParams)

let private sortableLink columnName (label: string) query hxIncludes =
    let url = $"""/organizations/summaries{buildQueryForSorting (columnName, query)}"""
    Fragment() {
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
            match query.SortBy with
            | Some(sort, dir) when sort = columnName && dir = Direction.Asc -> "▲"
            | Some(sort, _) when sort = columnName -> "▼"
            | _ -> ""
        }
    }

let createSortableBy (columnName: string) (label: string) (filter: Query)=
     div (style="display:flex;") {
         sortableLink columnName label filter """[name='liczba_beneficjentow'], [name='liczba_beneficjentow_op'], [name='search']"""
     }
    
let createFilterableSortableBy (columnName: string) (labelText: string) query =
    let columnFilter = query.Filters |> List.tryFind(fun filter -> filter.Key = columnName)
    div (style="display:flex;justify-content:space-between;") {
        sortableLink columnName labelText query """[name='liczba_beneficjentow'], [name='liczba_beneficjentow_op'], [name='search']"""
        let icon = match columnFilter with
                   | Some _ -> Icons.Filter
                   | None -> Icons.FilterEmpty
        let hasOp op = Option.exists (fun cf -> cf.Operator = op)  
        DropDown (24, icon) (Placement.Bottom, div(style="width:300px;") {
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