module Import.Router

open System.Security.Claims
open Oxpecker
open RenderBasedOnHtmx
    
let import: EndpointHandler =
    fun ctx -> task {
        let username = ctx.User.FindFirstValue(ClaimTypes.Name)
        return ctx |> render (ImportFileTemplate.Partial username) (ImportFileTemplate.FullPage username)
    }

let Endpoints = [
    GET [
        route "/" import
    ]
]
