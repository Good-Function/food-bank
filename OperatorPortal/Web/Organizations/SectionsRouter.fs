module Organizations.SectionsRouter

open Microsoft.AspNetCore.Http
open Organizations.Application
open Organizations.Templates
open Oxpecker
open Organizations.Application.ReadModels
open Organizations.CompositionRoot
open HttpContextExtensions

let daneAdresowe (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(DaneAdresowe.View details.DaneAdresowe teczka)
        }

let daneAdresoweEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(DaneAdresowe.Form details.DaneAdresowe teczka)
        }

let changeDaneAdresowe (handle: CommandHandlers.ChangeDaneAdresowe) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindForm<Commands.DaneAdresowe>()
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(DaneAdresowe.View (cmd |> ReadModels.DaneAdresowe.FromCommand) teczka)
        }

let kontakty (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Kontakty.View details.Kontakty teczka)
        }

let kontaktyEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Kontakty.Form details.Kontakty teczka)
        }

let changeKontakty (handle: CommandHandlers.ChangeKontakty) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindForm<Commands.Kontakty>()
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(Kontakty.View (cmd |> ReadModels.Kontakty.FromCommand) teczka)
        }
        
let beneficjenci (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Beneficjenci.View details.Beneficjenci teczka)
        }

let beneficjenciEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Beneficjenci.Form details.Beneficjenci teczka)
        }

let changeBeneficjenci (handle: CommandHandlers.ChangeBeneficjenci) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindForm<Commands.Beneficjenci>()
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(Beneficjenci.View (cmd |> ReadModels.Beneficjenci.FromCommand) teczka)
        }
        
let dokumenty (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Dokumenty.View details.Dokumenty teczka)
        }

let dokumentyEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Dokumenty.Form details.Dokumenty teczka)
        }

let changeDokumenty (handle: CommandHandlers.ChangeDokumenty) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindForm<Commands.Dokumenty>()
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(Dokumenty.View (cmd |> ReadModels.Dokumenty.FromCommand) teczka)
        }
        
let uploadWniosek (handle: CommandHandlers.UploadDocument) (teczka: int64) :EndpointHandler =
    fun ctx -> task {
        match ctx.TryGetFirstFile with
        | Some file ->
            do! handle (teczka, { Name = file.FileName; ContentStream = file.OpenReadStream() })
            return! ctx.WriteHtmlString ("OK")
        | None ->
            ctx.SetStatusCode(StatusCodes.Status400BadRequest)
            return! ctx.WriteHtmlString ("Błąd")
    }
        
let zrodlaZywnosci (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(ZrodlaZywnosci.View details.ZrodlaZywnosci teczka)
        }

let zrodlaZywnosciEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(ZrodlaZywnosci.Form details.ZrodlaZywnosci teczka)
        }

let changeZrodlaZywnosci (handle: CommandHandlers.ChangeZrodlaZywnosci) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindForm<Commands.ZrodlaZywnosci>()
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(ZrodlaZywnosci.View (cmd |> ReadModels.ZrodlaZywnosci.FromCommand) teczka)
        }
        
let adresyKsiegowosci (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(AdresyKsiegowosci.View details.AdresyKsiegowosci teczka)
        }

let adresyKsiegowosciEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(AdresyKsiegowosci.Form details.AdresyKsiegowosci teczka)
        }

let changeAdresyKsiegowosci (handle: CommandHandlers.ChangeAdresyKsiegowosci) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindForm<Commands.AdresyKsiegowosci>()
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(AdresyKsiegowosci.View (cmd |> ReadModels.AdresyKsiegowosci.FromCommand) teczka)
        }
     
let warunkiPomocy (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(WarunkiPomocy.View details.WarunkiPomocy teczka)
        }
        
let warunkiPomocyEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(WarunkiPomocy.Form details.WarunkiPomocy teczka)
        }
        
let changeWarunkiPomocy (handle: CommandHandlers.ChangeWarunkiPomocy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindForm<Commands.WarunkiPomocy>()
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(WarunkiPomocy.View (cmd |> ReadModels.WarunkiPomocy.FromCommand) teczka)
        }

let Endpoints (dependencies: Dependencies) =
    [ GET
          [ 
            routef "/{%d}/dane-adresowe" (daneAdresowe dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/dane-adresowe/edit" (daneAdresoweEdit dependencies.ReadOrganizationDetailsBy) 
            routef "/{%d}/kontakty" (kontakty dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/kontakty/edit" (kontaktyEdit dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/beneficjenci" (beneficjenci dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/beneficjenci/edit" (beneficjenciEdit dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/dokumenty" (dokumenty dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/dokumenty/edit" (dokumentyEdit dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/zrodla-zywnosci" (zrodlaZywnosci dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/zrodla-zywnosci/edit" (zrodlaZywnosciEdit dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/adresy-ksiegowosci" (adresyKsiegowosci dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/adresy-ksiegowosci/edit" (adresyKsiegowosciEdit dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/warunki-pomocy" (warunkiPomocy dependencies.ReadOrganizationDetailsBy)
            routef "/{%d}/warunki-pomocy/edit" (warunkiPomocyEdit dependencies.ReadOrganizationDetailsBy)
          ]
      PUT [
          routef "/{%d}/dane-adresowe" (changeDaneAdresowe dependencies.ChangeDaneAdresowe)
          routef "/{%d}/kontakty" (changeKontakty dependencies.ChangeKontakty)
          routef "/{%d}/beneficjenci" (changeBeneficjenci dependencies.ChangeBeneficjenci)
          routef "/{%d}/dokumenty" (changeDokumenty dependencies.ChangeDokumenty)
          routef "/{%d}/dokumenty/wniosek" (uploadWniosek dependencies.UploadDocument)
          routef "/{%d}/zrodla-zywnosci" (changeZrodlaZywnosci dependencies.ChangeZrodlaZywnosci)
          routef "/{%d}/adresy-ksiegowosci" (changeAdresyKsiegowosci dependencies.ChangeAdresyKsiegowosci)
          routef "/{%d}/warunki-pomocy" (changeWarunkiPomocy dependencies.ChangeWarunkiPomocy)
      ] ]

