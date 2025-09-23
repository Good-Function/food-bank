module Organizations.DataApi

open Microsoft.AspNetCore.Http
open Organizations.Application.ReadModels.OrganizationDetails
open Oxpecker

let kontakty (readDetailsBy: ReadOrganizationDetailsByEmail) : EndpointHandler =
    fun ctx ->
        task {
            match ctx.TryGetQueryValue("email") with
            | Some email -> 
                let! details = readDetailsBy email
                return! ctx.WriteJson(details.Kontakty)
            | None ->
                ctx.SetStatusCode(StatusCodes.Status400BadRequest)
                return! ctx.WriteText("Bad Request, Email is required")
        }
        
let warunkiPomocy (readDetailsBy: ReadOrganizationDetailsByEmail) : EndpointHandler =
    fun ctx ->
        task {
            match ctx.TryGetQueryValue("email") with
            | Some email -> 
                let! details = readDetailsBy email
                return! ctx.WriteJson(details.WarunkiPomocy)
            | None ->
                ctx.SetStatusCode(StatusCodes.Status400BadRequest)
                return! ctx.WriteText("Bad Request, Email is required")
        }
        
let beneficjenci (readDetailsBy: ReadOrganizationDetailsByEmail) : EndpointHandler =
    fun ctx ->
        task {
            match ctx.TryGetQueryValue("email") with
            | Some email -> 
                let! details = readDetailsBy email
                return! ctx.WriteJson(details.Beneficjenci)
            | None ->
                ctx.SetStatusCode(StatusCodes.Status400BadRequest)
                return! ctx.WriteText("Bad Request, Email is required")
        }
        
let zrodlaZywnosci (readDetailsBy: ReadOrganizationDetailsByEmail) : EndpointHandler =
    fun ctx ->
        task {
            match ctx.TryGetQueryValue("email") with
            | Some email -> 
                let! details = readDetailsBy email
                return! ctx.WriteJson(details.ZrodlaZywnosci)
            | None ->
                ctx.SetStatusCode(StatusCodes.Status400BadRequest)
                return! ctx.WriteText("Bad Request, Email is required")
        }
        
let daneAdresowe (readDetailsBy: ReadOrganizationDetailsByEmail) : EndpointHandler =
    fun ctx ->
        task {
            match ctx.TryGetQueryValue("email") with
            | Some email -> 
                let! details = readDetailsBy email
                return! ctx.WriteJson(details.DaneAdresowe)
            | None ->
                ctx.SetStatusCode(StatusCodes.Status400BadRequest)
                return! ctx.WriteText("Bad Request, Email is required")
        }

let Endpoints (dependencies: CompositionRoot.DataApiDependencies) =
    [ GET
          [
            route "/kontakty" (kontakty dependencies.ReadOrganizationDetailsByEmail)
            route "/beneficjenci" (beneficjenci dependencies.ReadOrganizationDetailsByEmail)
            route "/dane-adresowe" (daneAdresowe dependencies.ReadOrganizationDetailsByEmail)
            route "/zrodla-zywnosci" (zrodlaZywnosci dependencies.ReadOrganizationDetailsByEmail)
            route "/warunki-pomocy" (warunkiPomocy dependencies.ReadOrganizationDetailsByEmail)
          ]
    ]