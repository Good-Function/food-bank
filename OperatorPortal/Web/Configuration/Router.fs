module Configuration.Router

open Oxpecker
open RenderBasedOnHtmx

let passwordReset: EndpointHandler =
    fun ctx -> task {
        return ctx |> render ResetPassword.Partial ResetPassword.FullPage
    }
    
let csvImport: EndpointHandler =
    fun ctx -> task {
        return ctx |> render ImportCsvTemplate.Partial ImportCsvTemplate.FullPage
    }

let Endpoints= [
    GET [
        route "/password-reset" passwordReset
        route "/csv-import" csvImport
    ]
]
