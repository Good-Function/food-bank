module Organizations.SectionsRouter

open System
open HttpContextExtensions
open Microsoft.AspNetCore.Http
open Organizations.Application
open Organizations.Application.DocumentType
open Organizations.Templates
open Oxpecker
open FsToolkit.ErrorHandling
open Organizations.Application.ReadModels.OrganizationDetails
open Organizations.CompositionRoot
open Permissions

let daneAdresowe (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(DaneAdresowe.View details.DaneAdresowe teczka permissions)
        }

let daneAdresoweEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            let token = ctx.GetAntiforgeryInput()
            return ctx.WriteHtmlView(DaneAdresowe.Form details.DaneAdresowe teczka token)
        }

let changeDaneAdresowe (handle: Handlers.ChangeDaneAdresowe) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! cmd = ctx.BindForm<Commands.DaneAdresowe>()
            let! _ = handle(teczka, {Who = ctx.UserName; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.WriteHtmlView(DaneAdresowe.View (cmd |> DaneAdresowe.FromCommand) teczka permissions)
        }

let kontakty (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Kontakty.View details.Kontakty teczka permissions)
        }

let kontaktyEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            let token = ctx.GetAntiforgeryInput()
            return ctx.WriteHtmlView(Kontakty.Form details.Kontakty teczka token)
        }

let changeKontakty (handle: Handlers.ChangeKontakty) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! cmd = ctx.BindForm<Commands.Kontakty>()
            let! _ = handle(teczka, {Who = ctx.UserName; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.WriteHtmlView(Kontakty.View (cmd |> Kontakty.FromCommand) teczka permissions)
        }
        
let beneficjenci (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Beneficjenci.View details.Beneficjenci teczka permissions)
        }

let beneficjenciEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            let token = ctx.GetAntiforgeryInput()
            return ctx.WriteHtmlView(Beneficjenci.Form details.Beneficjenci teczka token)
        }

let changeBeneficjenci (handle: Handlers.ChangeBeneficjenci) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! cmd = ctx.BindForm<Commands.Beneficjenci>()
            let! _ = handle(teczka, {Who = ctx.UserName; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.WriteHtmlView(Beneficjenci.View (cmd |> Beneficjenci.FromCommand) teczka permissions)
        }
        
let dokumenty (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(Dokumenty.View details.Dokumenty teczka permissions)
        }
        
let downloadFile (handle: DocumentHandlers.GenerateDownloadUri) (teczka: int64) (fileName:string): EndpointHandler =
    let uri = handle (teczka, fileName)
    redirectTo uri.AbsoluteUri false

let dokumentyEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            let token = ctx.GetAntiforgeryInput()
            return ctx.WriteHtmlView(Dokumenty.Form details.Dokumenty teczka token)
        }
        
let readFileByType<'T> (ctx: HttpContext) (docType: 'T) fallbackFileName =
    let file = ctx.Request.Form.Files.Item $"{docType}" |> Option.ofObj
    let fileName = file |> Option.map _.FileName |> Option.orElse fallbackFileName
    file, fileName

let changeDokumenty (saveDocument: DocumentHandlers.SaveFile)
                    (deleteDocument: DocumentHandlers.DeleteFile)
                    (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! cmd = ctx.BindForm<Commands.Dokumenty>()
            let readFile = readFileByType ctx
            let wniosek, wniosekFileName = readFile Wniosek cmd.Wniosek       
            let umowa, umowaFileName = readFile Umowa cmd.Umowa
            let rodo, rodoFileName = readFile RODO cmd.RODO
            let odwiedziny, odwiedzinyFileName = readFile Odwiedziny cmd.Odwiedziny
            let upowaznienie, upowaznienieFileName = readFile UpowaznienieDoOdbioru cmd.UpowaznienieDoOdbioru
            
            do!
                [
                     cmd.DeleteOdwiedziny
                     cmd.DeleteUmowa
                     cmd.DeleteWniosek
                     cmd.DeleteUpowaznienieDoOdbioru
                     cmd.DeleteRODO
                ] |> List.choose id
                  |> List.map(fun fileName -> deleteDocument(teczka, fileName))
                  |> Async.Parallel
                  |> Async.Ignore    
            
            do! saveDocument(teczka, {
                Date = cmd.WniosekDate
                Type = Wniosek
                ContentStream = wniosek |> Option.map(_.OpenReadStream())
                FileName = wniosekFileName
            })
            do! saveDocument(teczka, {
                Date = cmd.RODODate
                Type = RODO
                ContentStream = rodo |> Option.map(_.OpenReadStream())
                FileName = rodoFileName
            })
            do! saveDocument(teczka, {
                Date = cmd.OdwiedzinyDate
                Type = Odwiedziny
                ContentStream = odwiedziny |> Option.map(_.OpenReadStream())
                FileName = odwiedzinyFileName
            })
            do! saveDocument(teczka, {
                Date = cmd.UpowaznienieDoOdbioruDate
                Type = UpowaznienieDoOdbioru
                ContentStream = upowaznienie |> Option.map(_.OpenReadStream())
                FileName = upowaznienieFileName
            })
            do! saveDocument(teczka, {
                Date = cmd.UmowaDate
                Type = Umowa
                ContentStream = umowa |> Option.map(_.OpenReadStream())
                FileName = umowaFileName
            })
            
            let documents: Document list = [
                { Date = cmd.WniosekDate; FileName = wniosekFileName; Type = Wniosek }
                { Date = cmd.UmowaDate; FileName = umowaFileName; Type = Umowa }
                { Date = cmd.OdwiedzinyDate; FileName = odwiedzinyFileName; Type = Odwiedziny }
                { Date = cmd.RODODate; FileName = rodoFileName; Type = RODO}
                { Date = cmd.UpowaznienieDoOdbioruDate; FileName = upowaznienieFileName; Type = UpowaznienieDoOdbioru }
            ]
            return ctx.WriteHtmlView(Dokumenty.View documents teczka permissions)
        }
        
let zrodlaZywnosci (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(ZrodlaZywnosci.View details.ZrodlaZywnosci teczka permissions)
        }

let zrodlaZywnosciEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64) : EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            let token = ctx.GetAntiforgeryInput()
            return ctx.WriteHtmlView(ZrodlaZywnosci.Form details.ZrodlaZywnosci teczka token)
        }

let changeZrodlaZywnosci (handle: Handlers.ChangeZrodlaZywnosci) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! cmd = ctx.BindForm<Commands.ZrodlaZywnosci>()
            let! _ = handle(teczka, {Who = ctx.UserName; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.WriteHtmlView(ZrodlaZywnosci.View (cmd |> ZrodlaZywnosci.FromCommand) teczka permissions)
        }
        
let adresyKsiegowosci (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(AdresyKsiegowosci.View details.AdresyKsiegowosci teczka permissions)
        }

let adresyKsiegowosciEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            let token = ctx.GetAntiforgeryInput()
            return ctx.WriteHtmlView(AdresyKsiegowosci.Form details.AdresyKsiegowosci teczka token)
        }

let changeAdresyKsiegowosci (handle: Handlers.ChangeAdresyKsiegowosci) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! cmd = ctx.BindForm<Commands.AdresyKsiegowosci>()
            let! _ = handle(teczka, {Who = ctx.UserName; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.WriteHtmlView(AdresyKsiegowosci.View (cmd |> AdresyKsiegowosci.FromCommand) teczka permissions)
        }
     
let warunkiPomocy (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! details = readDetailsBy teczka
            return ctx.WriteHtmlView(WarunkiPomocy.View details.WarunkiPomocy teczka permissions)
        }
        
let warunkiPomocyEdit (readDetailsBy: ReadOrganizationDetailsBy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! details = readDetailsBy teczka
            let token = ctx.GetAntiforgeryInput()
            return ctx.WriteHtmlView(WarunkiPomocy.Form details.WarunkiPomocy teczka token)
        }
        
let changeWarunkiPomocy (handle: Handlers.ChangeWarunkiPomocy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let permissions = permissionsMap[ctx.UserRole]
            let! cmd = ctx.BindForm<Commands.WarunkiPomocy>()
            let! _ = handle(teczka, {Who = ctx.UserName; OccuredAt = DateTime.UtcNow}, cmd)
            return ctx.WriteHtmlView(WarunkiPomocy.View (cmd |> WarunkiPomocy.FromCommand) teczka permissions)
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
          routef "/{%d}/dane-adresowe" (authorize Permission.EditOrganization >>=>
                                        changeDaneAdresowe dependencies.ChangeDaneAdresowe)
          routef "/{%d}/kontakty" (authorize Permission.EditOrganization >>=>
                                   changeKontakty dependencies.ChangeKontakty)
          routef "/{%d}/beneficjenci" (authorize Permission.EditOrganization >>=>
                                       changeBeneficjenci dependencies.ChangeBeneficjenci)
          routef "/{%d}/dokumenty" (authorize Permission.EditOrganization >>=>
                                    changeDokumenty dependencies.SaveDocument dependencies.DeleteDocument)
          routef "/{%d}/zrodla-zywnosci" (authorize Permission.EditOrganization >>=>
                                          changeZrodlaZywnosci dependencies.ChangeZrodlaZywnosci)
          routef "/{%d}/adresy-ksiegowosci" (authorize Permission.EditOrganization >>=>
                                             changeAdresyKsiegowosci dependencies.ChangeAdresyKsiegowosci)
          routef "/{%d}/warunki-pomocy" (authorize Permission.EditOrganization >>=>
                                         changeWarunkiPomocy dependencies.ChangeWarunkiPomocy)
      ] ]

