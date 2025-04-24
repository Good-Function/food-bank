module Organizations.SectionsRouter

open Organizations.Application
open Organizations.Templates
open Organizations.Dtos
open Oxpecker
open Organizations.Application.ReadModels
open Organizations.CompositionRoot

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

let changeDaneAdresowe (handle: ChangeOrgazniationCommands.ChangeDaneAdresowe) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<DaneAdresoweForm>()
            let cmd = form.toChangeDaneAdresowe
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(DaneAdresowe.View form.toDaneAdresowe teczka)
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

let changeKontakty (handle: ChangeOrgazniationCommands.ChangeKontakty) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<KontaktyForm>()
            let cmd = form.toChangeKontakty
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(Kontakty.View form.toKontakty teczka)
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

let changeBeneficjenci (handle: ChangeOrgazniationCommands.ChangeBeneficjenci) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<BeneficjenciForm>()
            let cmd = form.toChangeBeneficjenci
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(Beneficjenci.View form.toBeneficjenci teczka)
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

let changeDokumenty (handle: ChangeOrgazniationCommands.ChangeDokumenty) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<DokumentyForm>()
            let cmd = form.toChangeDokumenty
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(Dokumenty.View form.toDokumenty teczka)
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

let changeZrodlaZywnosci (handle: ChangeOrgazniationCommands.ChangeZrodlaZywnosci) (teczka: int64) :EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<ZrodlaZywnosciForm>()
            let cmd = form.toChangeZrodlaZywnosci
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(ZrodlaZywnosci.View form.toZrodlaZywnosci teczka)
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

let changeAdresyKsiegowosci (handle: ChangeOrgazniationCommands.ChangeAdresyKsiegowosci) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<AdresyKsiegowosciForm>()
            let cmd = form.toChangeAdresyKsiegowosci
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(AdresyKsiegowosci.View form.toAdresyKsiegowosci teczka)
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
        
let changeWarunkiPomocy (handle: ChangeOrgazniationCommands.ChangeWarunkiPomocy) (teczka: int64): EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<WarunkiPomocyForm>()
            let cmd = form.toChangeWarunkiPomocy
            do! handle(teczka, cmd)
            return ctx.WriteHtmlView(WarunkiPomocy.View form.toWarunkiPomocy teczka)
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
          routef "/{%d}/zrodla-zywnosci" (changeZrodlaZywnosci dependencies.ChangeZrodlaZywnosci)
          routef "/{%d}/adresy-ksiegowosci" (changeAdresyKsiegowosci dependencies.ChangeAdresyKsiegowosci)
          routef "/{%d}/warunki-pomocy" (changeWarunkiPomocy dependencies.ChangeWarunkiPomocy)
      ] ]

