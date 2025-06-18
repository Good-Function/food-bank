module Organizations.Templates.List.Filterable

open Layout
open Organizations.Application.ReadModels.FilterOperators
open Organizations.Application.ReadModels.QueriedColumn
open Organizations.Application.ReadModels.OrganizationSummary
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Web.Layout.Dropdown

type FilterType =
    | StringFilter
    | NumberFilter
    
type FilterBy = {
    Type: FilterType
    Column: QueriedColumn
    CurrentFilters: Filter list
}

let hasOperator (op: FilterOperator) =
    Option.exists (fun cf -> cf.Operator = op)
    
let textFilterPopover (filterBy: FilterBy) =
    let columnFilter = filterBy.CurrentFilters |> List.tryFind(fun filter -> filter.Key = filterBy.Column)
    div(style="width:200px;") {
            div(
                style="display:flex; flex-direction:column; align-items: baseline; justify-content: space-between"
                ) {
                span (style="white-space:no-break; margin-bottom: var(--pico-spacing") { filterBy.Column.Label }
                select (
                    name = $"{filterBy.Column}_op",
                    style = "margin: 0; padding-left: 5px; padding-top: 0; padding-bottom: 5px; font-weight:normal"
                ) {
                    for operator in textOperators do
                        let isSelected = columnFilter |> hasOperator (FilterOperator.TextOperator operator)
                        option(selected = isSelected) { operator.Label }
                }
                input (
                    hxTrigger = "keyup[key=='Enter']",
                    hxTarget = "#OrganizationsList",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = $"[name='sort'], [name='dir'], [name='search'], {HxIncludes.all}",
                    style = "margin: 0",
                    value = (columnFilter |> Option.map _.Value.ToString() |> Option.defaultValue ""),
                    name = $"{filterBy.Column}",
                    type'= "text")
            }
            div(style="display: flex; justify-content:space-between; gap:var(--pico-spacing); margin-top:var(--pico-spacing); font-weight:normal"){
                div(
                    style="cursor:pointer",
                    hxTarget = "#OrganizationsList",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = $"[name='sort'], [name='dir'], [name='search'], {HxIncludes.allExcept filterBy.Column}"
                    ){
                    Icons.Cancel
                    "usuń"
                }
                div(
                    style="cursor:pointer",
                    hxTarget = "#OrganizationsList",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = $"[name='sort'], [name='dir'], [name='search'], {HxIncludes.all}"
                    ){
                    "zastosuj"
                    Icons.Ok
                }
            }
        } 
    
let numberFilterPopover (filterBy: FilterBy) =
    let columnFilter = filterBy.CurrentFilters |> List.tryFind(fun filter -> filter.Key = filterBy.Column)
    div(style="width:300px;") {
            div(
                style="display:flex; align-items: baseline; justify-content: space-between"
                ) {
                span (style="white-space:no-break") { filterBy.Column.Label }
                select (
                    name = $"{filterBy.Column}_op",
                    style = "height: 37px; margin: 0; padding-right: 5px; padding-top: 0; padding-bottom: 0; width: 100px;"
                ) {
                    for operator in numberOperators do
                        let isSelected = columnFilter |> hasOperator (FilterOperator.NumberOperator operator)
                        option(selected = isSelected) { operator.Label }
                }
                input (
                    hxTrigger = "keyup[key=='Enter']",
                    hxTarget = "#OrganizationsList",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = $"[name='sort'], [name='dir'], [name='search'], {HxIncludes.all}",
                    style="width: 100px; margin-bottom:0;",
                    value = (columnFilter |> Option.map _.Value.ToString() |> Option.defaultValue ""),
                    name = $"{filterBy.Column}",
                    type'= "number")
            }
            div(style="display: flex; justify-content:space-between; gap:var(--pico-spacing); margin-top:var(--pico-spacing); font-weight:normal"){
                div(
                    style="cursor:pointer",
                    hxTarget = "#OrganizationsList",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = $"[name='sort'], [name='dir'], [name='search'], {HxIncludes.allExcept filterBy.Column}"
                    ){
                    Icons.Cancel
                    "usuń"
                }
                div(
                    style="cursor:pointer",
                    hxTarget = "#OrganizationsList",
                    hxGet = "/organizations/summaries",
                    hxPushUrl ="true",
                    hxIndicator = ".big-table",
                    hxInclude = $"[name='sort'], [name='dir'], [name='search'], {HxIncludes.all}"
                    ){
                    "zastosuj"
                    Icons.Ok
                }
            }
        }
        
    
let filterable (filterBy: FilterBy) =
    let columnFilter = filterBy.CurrentFilters |> List.tryFind(fun filter -> filter.Key = filterBy.Column)
    let icon = match columnFilter with
               | Some _ -> Icons.Filter
               | None -> Icons.FilterEmpty
    let filter = 
        match filterBy.Type with
        | NumberFilter -> numberFilterPopover filterBy
        | StringFilter -> textFilterPopover filterBy
    DropDown (24, icon) (Placement.Bottom, filter)