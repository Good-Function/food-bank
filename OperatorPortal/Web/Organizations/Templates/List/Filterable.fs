module Web.Organizations.Templates.List.Filterable

open Layout
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Layout.Dropdown

type FilterType =
    | StringFilter
    | NumberFilter
    
type FilterBy = {
    Type: FilterType
    ColumnKey: string
    FilterLabel: string
    CurrentFilters: Filter list
}
let hasOp op = Option.exists (fun cf -> cf.Operator = op)
let opearators (currentFilter: Filter option) (filterType: FilterType) =
    Fragment() {
        match filterType with
            | NumberFilter ->
                option(selected = (currentFilter |> hasOp "=")) { "=" }
                option(selected = (currentFilter |> hasOp ">")) { ">" }
                option(selected = (currentFilter |> hasOp "<")) { "<" }
                option(selected = (currentFilter |> hasOp ">=")) { ">=" }
                option(selected = (currentFilter |> hasOp "<=")) { "<=" }
            | StringFilter ->
                option(selected = (currentFilter |> hasOp "zawiera")) { "zawiera" }
                option(selected = (currentFilter |> hasOp "nie zawiera")) { "nie zawiera" }
    }
        
    
let filterable (filterBy: FilterBy) =
    let columnFilter = filterBy.CurrentFilters |> List.tryFind(fun filter -> filter.Key = filterBy.ColumnKey)
    let icon = match columnFilter with
               | Some _ -> Icons.Filter
               | None -> Icons.FilterEmpty
    DropDown (24, icon) (Placement.Bottom, div(style="width:300px;") {
            div(
                style="display:flex; align-items: baseline; justify-content: space-between"
                ) {
                span (style="white-space:no-break") { filterBy.FilterLabel }
                select (
                    name = $"{filterBy.ColumnKey}_op",
                    style = "height: 34.5px; margin: 0; padding-right: 5px; padding-top: 0; padding-bottom: 0; width: 100px;"
                ) {
                    opearators columnFilter filterBy.Type
                }
                input (
                    hxTrigger = "keyup[key=='Enter']",
                    hxTarget = "#OrganizationsList",
                    hxSwap = "outerHTML",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = $"[name='sort'], [name='dir'], [name='{filterBy.ColumnKey}_op'], [name='search']",
                    style="width: 100px; margin-bottom:0;",
                    value = (columnFilter |> Option.map _.Value.ToString() |> Option.defaultValue ""),
                    name = filterBy.ColumnKey,
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
                    hxInclude = $"[name='sort'], [name='dir'], [name='{filterBy.ColumnKey}'], [name='{filterBy.ColumnKey}_op'], [name='search']"
                    ){
                    "zastosuj"
                    Icons.Ok
                }
            }
        })