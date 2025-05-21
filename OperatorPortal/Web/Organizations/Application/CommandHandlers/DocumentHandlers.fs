module Organizations.Application.DocumentHandlers

open System
open System.IO
open Organizations.Application
open Organizations.Application.DocumentType

type FileName = string

type DocumentUpload = { ContentStream: Stream; FileName: string }
type DocumentMetadata = {
    Date: DateOnly option
    FileName: string option
}
type Document = {
    Date: DateOnly option
    FileName: string option
    ContentStream: Stream option
    Type: DocumentType
}

type SaveWniosekMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type SaveUpowaznienieDoOdbioruMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type SaveRodoMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type SaveUmowaMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type SaveOdwiedzinyMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type DeleteWniosekMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type DeleteUpowaznienieDoOdbioruMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type DeleteRodoMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type DeleteUmowaMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type DeleteOdwiedzinyMetadata = Commands.TeczkaId * DocumentMetadata -> Async<unit>
type DeleteFile = Commands.TeczkaId * FileName -> Async<unit>
type UploadBlob = Commands.TeczkaId * DocumentUpload -> Async<unit>
type SaveFile = Commands.TeczkaId * Document -> Async<unit>

let uploadWniosekHandler (upload: UploadBlob) (saveMetadata: SaveWniosekMetadata) : SaveFile =
    fun (teczkaId, document) -> async {
        match (document.FileName, document.ContentStream) with
        | Some fileName, Some stream -> do! upload(teczkaId, { FileName = fileName; ContentStream = stream })
        | _ -> ()
        do! saveMetadata(teczkaId, { FileName = document.FileName; Date = document.Date })
    }