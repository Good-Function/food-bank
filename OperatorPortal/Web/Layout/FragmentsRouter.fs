module Layout.FragmentsRouter

open Oxpecker

let deletedFileHandler: EndpointHandler =
    fun ctx ->
        let fieldName =
            match ctx.TryGetQueryValue "inputName" with
            | None -> ""
            | Some q -> q
        let fileName =
            match ctx.TryGetQueryValue "fileName" with
            | None -> ""
            | Some q -> q
        ctx.WriteHtmlView (DeletedFileInput.View (fieldName, fileName))

let Endpoints = [ route "/file-input-after-delete" deletedFileHandler ]