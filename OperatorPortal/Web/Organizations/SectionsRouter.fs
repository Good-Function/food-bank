module Organizations.SectionsRouter

open Microsoft.AspNetCore.Http
open Organizations.Application
open Organizations.Application.DocumentType
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

let changeDaneAdresowe (handle: Handlers.ChangeDaneAdresowe) (teczka: int64) :EndpointHandler =
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

let changeKontakty (handle: Handlers.ChangeKontakty) (teczka: int64) :EndpointHandler =
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

let changeBeneficjenci (handle: Handlers.ChangeBeneficjenci) (teczka: int64) :EndpointHandler =
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
        
let downloadFile (handle: Handlers.GenerateDownloadUri) (teczka: int64) (fileName:string): EndpointHandler =
    let uri = handle (teczka, fileName)
    redirectTo (uri.ToString()) false

let dokumentyEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Dokumenty.Form details.Dokumenty teczka)
        }

let changeDokumenty (
    saveWniosek: DocumentHandlers.SaveFile,
    saveRODO: DocumentHandlers.SaveFile,
    saveUpowaznienie: DocumentHandlers.SaveFile,
    saveUmowa: DocumentHandlers.SaveFile,
    saveOdwiedziny: DocumentHandlers.SaveFile
    ) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! cmd = ctx.BindForm<Commands.Dokumenty>()
            let wniosek = ctx.Request.Form.Files.Item $"{Wniosek}" |> Option.ofObj
            let umowa = ctx.Request.Form.Files.Item $"{Umowa}" |> Option.ofObj
            let rodo = ctx.Request.Form.Files.Item $"{RODO}" |> Option.ofObj
            let odwiedziny = ctx.Request.Form.Files.Item $"{Odwiedziny}" |> Option.ofObj
            let upowaznienie = ctx.Request.Form.Files.Item $"{UpowaznienieDoOdbioru}" |> Option.ofObj
            
            do! saveWniosek(teczka, {
                Date = cmd.WniosekDate
                Type = Wniosek
                ContentStream = wniosek |> Option.map(_.OpenReadStream())
                FileName = wniosek |> Option.map(_.FileName)
            })
            do! saveRODO(teczka, {
                Date = cmd.RODODate
                Type = RODO
                ContentStream = rodo |> Option.map(_.OpenReadStream())
                FileName = rodo |> Option.map(_.FileName)
            })
            do! saveOdwiedziny(teczka, {
                Date = cmd.OdwiedzinyDate
                Type = Odwiedziny
                ContentStream = odwiedziny |> Option.map(_.OpenReadStream())
                FileName = odwiedziny |> Option.map(_.FileName)
            })
            do! saveUpowaznienie(teczka, {
                Date = cmd.WniosekDate
                Type = UpowaznienieDoOdbioru
                ContentStream = upowaznienie |> Option.map(_.OpenReadStream())
                FileName = upowaznienie |> Option.map(_.FileName)
            })
            do! saveUmowa(teczka, {
                Date = cmd.UmowaDate
                Type = Umowa
                ContentStream = umowa |> Option.map(_.OpenReadStream())
                FileName = umowa |> Option.map(_.FileName)
            })
            
            return ctx.WriteHtmlString "OK"
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

let changeZrodlaZywnosci (handle: Handlers.ChangeZrodlaZywnosci) (teczka: int64) :EndpointHandler =
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

let changeAdresyKsiegowosci (handle: Handlers.ChangeAdresyKsiegowosci) (teczka: int64): EndpointHandler =
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
        
let changeWarunkiPomocy (handle: Handlers.ChangeWarunkiPomocy) (teczka: int64): EndpointHandler =
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
            routef "/{%d}/dokumenty/{%s}" (downloadFile dependencies.GenerateDownloadUri)
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
          // routef "/{%d}/dokumenty" (changeDokumenty dependencies.ChangeDokumenty)
          routef "/{%d}/zrodla-zywnosci" (changeZrodlaZywnosci dependencies.ChangeZrodlaZywnosci)
          routef "/{%d}/adresy-ksiegowosci" (changeAdresyKsiegowosci dependencies.ChangeAdresyKsiegowosci)
          routef "/{%d}/warunki-pomocy" (changeWarunkiPomocy dependencies.ChangeWarunkiPomocy)
      ] ]

