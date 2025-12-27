module Organizations.Templates.Dokumenty

open Layout
open Layout.Fields
open Organizations.Application.ReadModels.OrganizationDetails
open Oxpecker.ViewEngine
open Organizations.Templates.Formatters
open Oxpecker.Htmx
open Permissions

let private documentEdit (doc: Document)=
    tr() {
        td() {
            match doc.FileName with
            | None ->
                    input (
                        type' = "file",
                        name = $"{doc.Type}",
                        required = true,
                        accept = ".pdf",
                        style="padding: 0;margin: 0; height: 32px;",
                        class'="in-table-file-input"
                    )
            | Some name -> 
                div(style="display: inline-block; height: 32px; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; width: 100%;") {
                    input(type' = "hidden", name = $"{doc.Type}", value = $"{name}")
                    a(hxGet = $"/fragments/file-input-after-delete?inputName={doc.Type}&fileName={name}",
                      hxTarget = "closest td",
                      style="width:32px;display:inline-block;cursor:pointer;") {
                        Icons.Delete
                    }
                    name
                }
        }
        td() {doc.Type.toLabel}
        td() {
             dateField doc.Date $"{doc.Type}Date"
        }
    }

let private documentView (teczka: int64) (doc: Document) =
    tr() {
        td() {
            match doc.FileName with
            | None -> "-"
            | Some name -> 
                a( style="display: inline-block; height: 32px; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; width: 100%;",
                   href = $"/organizations/{teczka}/dokumenty/{name}", target="_blank") {
                    div(style="width:32px;display:inline-block;") {Icons.DownloadFile}
                    name
                }
        }
        td() {doc.Type.toLabel}
        td() {
            doc.Date |> toDisplay
        }
}

let View (documents: Document list) (teczka: int64) (permissions: Permission list) =
    article () {
        editableHeader "Dokumenty" $"/organizations/{teczka}/dokumenty/edit" permissions
        table(style="table-layout:fixed;") {
            thead() {
                tr() {
                    th() {"Nazwa"}
                    th() {"Typ"}
                    th() {"Ważny od"}
                }
            }
            tbody() {
                for doc in documents do
                    doc |> documentView teczka
            }
        }
    }

let Form (documents: Document list) (teczka: int64) (antiforgeryToken: HtmlElement) =
    let spinner = "DokumentySpinner"
    form () {
        antiforgeryToken
        article (class' = "focus-dim") {
            activeEditableHeader "Dokumenty" $"/organizations/{teczka}/dokumenty" spinner
            Indicators.OverlaySpinner "DokumentySpinner"
            table(style="table-layout:fixed;") {
                thead() {
                    tr() {
                        th() {"Nazwa"}
                        th() {"Typ"}
                        th() {"Ważny od"}
                    }
                }
                tbody() {
                    for doc in documents do
                        doc |> documentEdit
                }
            }
        }
    }
