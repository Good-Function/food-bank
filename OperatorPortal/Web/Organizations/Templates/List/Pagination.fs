module Organizations.Templates.List.Pagination

open Layout
open Organizations.Application.ReadModels.OrganizationSummary
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let build (pagination: Pagination) (length: int option) =
    let offset = (pagination.Page - 1) * pagination.Size
    let range = $"{offset + 1}-{offset + (length |> Option.defaultValue 0)}"

    Fragment () {
        span (style = "margin-right:.25rem") { $"{range} z" }
        span(
            hxTrigger="revealed",
            hxGet = "/organizations/summaries/count",
            hxInclude = $"[name='search'], [name='sort'], [name='dir'], {HxIncludes.all}"
            ) {"wielu"}
        button (class' = "outline secondary", disabled = true) { div () { Icons.ChevronLeft } }
        button (class' = "outline secondary") { div () { Icons.ChevronRight } }
    }
