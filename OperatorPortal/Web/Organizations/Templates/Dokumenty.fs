module Organizations.Templates.Dokumenty

open Layout
open Layout.Fields
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Organizations.Templates.Formatters
open Oxpecker.Htmx

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
                    a(hxGet = $"/fragments/file-input-after-delete?inputName={doc.Type}",
                      hxTarget = "closest td",
                      style="width:32px;display:inline-block;cursor:pointer;") {
                        Icons.Delete
                    }
                    name
                    // Po delete: tutaj zwrotka z inputem o potrzebnej nazwie + input hidden z plikem do usuniecia. Mamy to!
        // input(name = $"Delete{DocumentType.Odwiedziny}", value="false") // should swap to input(name=..., value="true"). Ale to oob trzeba.
        // input(name = $"Delete{DocumentType.Umowa}", value="false")
        // input(name = $"Delete{DocumentType.Wniosek}", value="false")
        // input(name = $"Delete{DocumentType.UpowaznienieDoOdbioru}", value="false")
        // input(name = $"Delete{DocumentType.RODO}", value="false")
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

let View (documents: Document list) (teczka: int64) =
    article () {
        editableHeader "Dokumenty" $"/organizations/{teczka}/dokumenty/edit"
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

let Form (documents: Document list) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Dokumenty" $"/organizations/{teczka}/dokumenty"
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
