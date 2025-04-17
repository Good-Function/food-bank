module Organizations.ImportExcelTemplate

open Layout
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open Oxpecker.ViewEngine.Aria

let Upload (error: string option) =
    form (style = "max-width:650px; margin:auto;", hxEncoding = "multipart/form-data", hxPost = "import/upload", hxIndicator="#spinner") {
        match error with
        | Some error ->
            Fragment() {
                input (
                    type' = "file",
                    name = "file",
                    required = true,
                    accept = ".xlsx, .pdf",
                    ariaInvalid = "true",
                    ariaDescribedBy = "file-error",
                    style="padding-inline-start: 0 !important;"
                )

                small (id = "file-error") { error }
            }
        | None -> input (type' = "file", name = "file", required = true, accept = ".xlsx, .pdf")

        input (type' = "submit", value = "Importuj")
        div(id="spinner", class'="htmx-indicator", style="text-align:center; height:50px;") {
            Icons.Spinner
        }
    }

let Partial (userName: string) (error: string option) =
    Fragment() {
        Body.Template (Upload error) None userName
        "Import File" |> Head.ReplaceTitle
    }

let FullPage (userName: string) (error: string option) =
    Head.Template (Partial userName error) "Import File"
