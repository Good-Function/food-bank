module Organizations.Templates.List.Pagination

open Layout
open Organizations.Application.ReadModels.OrganizationSummary
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let build (pagination: Pagination) (length: int option) =
    let offset = (pagination.Page - 1) * pagination.Size
    let range = $"{offset + 1}-{offset + (length |> Option.defaultValue 0)}"
    let disablePrevious = pagination.Page = 1
    // todo: fix by passing total here
    let disableNext = match length with
                      | Some length -> length % pagination.Size <> 0
                      | None -> true

    Fragment () {
        span (style = "margin-right:.25rem") { $"{range} z" }
        span(
            hxTrigger="revealed",
            hxGet = "/organizations/summaries/count",
            hxInclude = $"[name='search'], [name='sort'], [name='dir'], {HxIncludes.all}"
            ) {"wielu"}
        button (
            class' = "outline secondary",
            disabled = disablePrevious,
            hxPushUrl = "true",
            hxGet = $"/organizations/summaries?page={pagination.Page - 1}",
            hxTarget = "#OrganizationsList",
            hxInclude = $"[name='search'], [name='sort'], [name='dir'], {HxIncludes.all}"
            ) { div () { Icons.ChevronLeft } }
        button (
            class' = "outline secondary",
            disabled = disableNext,
            hxGet = $"/organizations/summaries?page={pagination.Page + 1}",
            hxPushUrl = "true",
            hxTarget = "#OrganizationsList",
            hxInclude = $"[name='search'], [name='sort'], [name='dir'], {HxIncludes.all}"
            ) { div () { Icons.ChevronRight } }
    }
