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
                ctx.SetStatusCode(StatusCodes.Status404NotFound)
                return! ctx.WriteText("Not found")
        }

let Endpoints (dependencies: CompositionRoot.DataApiDependencies) =
    [ GET
          [ route "/kontakty" (kontakty dependencies.ReadOrganizationDetailsByEmail)
          ]
    ]