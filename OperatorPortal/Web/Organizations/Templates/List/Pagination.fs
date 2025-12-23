module Organizations.Templates.List.Pagination

open Layout
open Organizations.Application.ReadModels.OrganizationSummary
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Oxpecker.Htmx

let build (pagination: Pagination) (currentRowsCount: int) (total: int)=
    let offset = (pagination.Page - 1) * pagination.Size
    let range = $"{offset + 1}-{offset + currentRowsCount}"
    let disablePrevious = pagination.Page = 1
    let disableNext = offset + currentRowsCount = total

    Fragment () {
        span (style = "margin-right:.25rem") { $"{range} z {total}" }
        button (
            class' = "outline secondary",
            ariaLabel = "Poprzednia strona",
            disabled = disablePrevious,
            hxPushUrl = "true",
            hxGet = $"/organizations/summaries?page={pagination.Page - 1}",
            hxIndicator = ".big-table",
            hxTarget = "#OrganizationsList",
            hxInclude = $"[name='search'], [name='sort'], [name='dir'], {HxIncludes.all}"
            ) { div () { Icons.ChevronLeft } }
        button (
            class' = "outline secondary",
            ariaLabel = "Następna strona",
            disabled = disableNext,
            hxGet = $"/organizations/summaries?page={pagination.Page + 1}",
            hxIndicator = ".big-table",
            hxPushUrl = "true",
            hxTarget = "#OrganizationsList",
            hxInclude = $"[name='search'], [name='sort'], [name='dir'], {HxIncludes.all}"
            ) { div () { Icons.ChevronRight } }
    }
