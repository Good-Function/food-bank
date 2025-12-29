module Organizations.DataApi

open System
open Microsoft.AspNetCore.Http
open Organizations.Application
open Organizations.Application.ReadModels.OrganizationDetails
open Oxpecker

let kontakty (readDetailsBy: ReadOrganizationDetailsBy) (teczkaId: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczkaId
            return! ctx.WriteJson details.Kontakty
        }
        
let adresyKsiegowosci (readDetailsBy: ReadOrganizationDetailsBy) (teczkaId: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczkaId
            return! ctx.WriteJson details.AdresyKsiegowosci
        }

let warunkiPomocy (readDetailsBy: ReadOrganizationDetailsBy) (teczkaId: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczkaId
            return! ctx.WriteJson details.WarunkiPomocy
        }

let beneficjenci (readDetailsBy: ReadOrganizationDetailsBy) (teczkaId: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczkaId
            return! ctx.WriteJson details.Beneficjenci
        }

let zrodlaZywnosci (readDetailsBy: ReadOrganizationDetailsBy) (teczkaId: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczkaId
            return! ctx.WriteJson details.ZrodlaZywnosci
        }

let daneAdresowe (readDetailsBy: ReadOrganizationDetailsBy) (teczkaId: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczkaId
            return! ctx.WriteJson details.DaneAdresowe
        }
        
let lookup (readDetailsBy: ReadOrganizationDetailsByEmail) : EndpointHandler =
    fun ctx ->
        task {
             let! body = ctx.BindJson<{|email: string|}>()
             match! readDetailsBy body.email with
             | Some org -> return! ctx.WriteJson {| id = org.Teczka; name = org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc |}
             | None -> return! ctx.WriteJson {| id = null; name = null |}
        }
        
let changeDaneAdresowe (handle: Handlers.ChangeDaneAdresowe) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindJson<Commands.DaneAdresowe>()
            let userMail = ctx.TryGetHeaderValue("X-User-Email")
                           |> Option.defaultValue "Pracownik organizacji charytatywnej"
            let! _ = handle(teczka, {Who = userMail; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.SetStatusCode(StatusCodes.Status204NoContent)
        }
        
let changeKontakty (handle: Handlers.ChangeKontakty) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindJson<Commands.Kontakty>()
            let userMail = ctx.TryGetHeaderValue("X-User-Email")
                           |> Option.defaultValue "Pracownik organizacji charytatywnej"
            let! _ = handle(teczka, {Who = userMail; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.SetStatusCode(StatusCodes.Status204NoContent)
        }
        
let changeBeneficjenci (handle: Handlers.ChangeBeneficjenci) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindJson<Commands.Beneficjenci>()
            let userMail = ctx.TryGetHeaderValue("X-User-Email")
                           |> Option.defaultValue "Pracownik organizacji charytatywnej"
            let! _ = handle(teczka, {Who = userMail; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.SetStatusCode(StatusCodes.Status204NoContent)
        }

let changeZrodlaZywnosci (handle: Handlers.ChangeZrodlaZywnosci) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindJson<Commands.ZrodlaZywnosci>()
            let userMail = ctx.TryGetHeaderValue("X-User-Email")
                           |> Option.defaultValue "Pracownik organizacji charytatywnej"
            let! _ = handle(teczka, {Who = userMail; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.SetStatusCode(StatusCodes.Status204NoContent)
        }
        
let changeAdresyKsiegowosci (handle: Handlers.ChangeAdresyKsiegowosci) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindJson<Commands.AdresyKsiegowosci>()
            let userMail = ctx.TryGetHeaderValue("X-User-Email")
                           |> Option.defaultValue "Pracownik organizacji charytatywnej"
            let! _ = handle(teczka, {Who = userMail; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.SetStatusCode(StatusCodes.Status204NoContent)
        }
        
let changeWarunkiPomocy (handle: Handlers.ChangeWarunkiPomocy) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindJson<Commands.WarunkiPomocy>()
            let userMail = ctx.TryGetHeaderValue("X-User-Email")
                           |> Option.defaultValue "Pracownik organizacji charytatywnej"
            let! _ = handle(teczka, {Who = userMail; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.SetStatusCode(StatusCodes.Status204NoContent)
        }

let Endpoints (deps: CompositionRoot.DataApiDependencies) =
    [ GET
          [ routef "/{%d}/kontakty" (kontakty deps.ReadOrganizationDetailsBy)
            routef "/{%d}/beneficjenci" (beneficjenci deps.ReadOrganizationDetailsBy)
            routef "/{%d}/dane-adresowe" (daneAdresowe deps.ReadOrganizationDetailsBy)
            routef "/{%d}/zrodla-zywnosci" (zrodlaZywnosci deps.ReadOrganizationDetailsBy)
            routef "/{%d}/adresy-ksiegowosci" (adresyKsiegowosci deps.ReadOrganizationDetailsBy)
            routef "/{%d}/warunki-pomocy" (warunkiPomocy deps.ReadOrganizationDetailsBy) ]
      POST [ route "/lookup-by-email" (lookup deps.ReadOrganizationDetailsByEmail) ]
      PUT [
          routef "/{%d}/dane-adresowe" (changeDaneAdresowe deps.ChangeDaneAdresowe)
          routef "/{%d}/kontakty" (changeKontakty deps.ChangeKontakty)
          routef "/{%d}/beneficjenci" (changeBeneficjenci deps.ChangeBeneficjenci)
          routef "/{%d}/zrodla-zywnosci" (changeZrodlaZywnosci deps.ChangeZrodlaZywnosci)
          routef "/{%d}/adresy-ksiegowosci" (changeAdresyKsiegowosci deps.ChangeAdresyKsiegowosci)
          routef "/{%d}/warunki-pomocy" (changeWarunkiPomocy deps.ChangeWarunkiPomocy)
      ]
    ]
