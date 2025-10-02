module Organizations.Application.Handlers

open FsToolkit.ErrorHandling
open Organizations.Application.Audit
open Organizations.Domain
open Organizations.Domain.Organization
open Organizations.Application
open Organizations.Domain.Identifiers
open Organizations.FindDiffForAudit

type Command<'EntityId, 'Payload> = 'EntityId * Audit * 'Payload
type ChangeKontakty = Command<Commands.TeczkaId, Commands.Kontakty> -> Async<Result<unit, TeczkaIdError>>
type ChangeZrodlaZywnosci = Command<Commands.TeczkaId, Commands.ZrodlaZywnosci> -> Async<Result<unit, TeczkaIdError>>
type ChangeAdresyKsiegowosci = Command<Commands.TeczkaId, Commands.AdresyKsiegowosci> -> Async<Result<unit, TeczkaIdError>>
type ChangeDaneAdresowe = Command<Commands.TeczkaId, Commands.DaneAdresowe> -> Async<Result<unit, TeczkaIdError>>
type ChangeBeneficjenci = Command<Commands.TeczkaId, Commands.Beneficjenci> -> Async<Result<unit, TeczkaIdError>>
type ChangeWarunkiPomocy = Command<Commands.TeczkaId, Commands.WarunkiPomocy> -> Async<Result<unit, TeczkaIdError>>

let changeWarunkiPomocy
    (readBy: TeczkaId -> Async<Organization.Organization>)
    (save: Organization.Organization -> Async<unit>)
    (track: AuditTrail -> Async<unit>) : ChangeWarunkiPomocy =
    fun (id, audit, cmd) ->
        let warunki = {
            HACCP = cmd.HACCP
            Kategoria = cmd.Kategoria
            RodzajPomocy = cmd.RodzajPomocy
            Sanepid = cmd.Sanepid
            SposobUdzielaniaPomocy = cmd.SposobUdzielaniaPomocy
            TransportKategoria = cmd.TransportKategoria
            TransportOpis = cmd.TransportOpis
            WarunkiMagazynowe = cmd.WarunkiMagazynowe
        }
        asyncResult {
            let! teczkaId = id |> TeczkaId.create
            let! organization = readBy teczkaId
            use transaction = openTransaction()
            let updatedOrg = { organization with WarunkiPomocy = warunki }
            do! save updatedOrg
            do! track {
                Who = audit.Who
                OccuredAt = audit.OccuredAt
                Kind = "Kontakty"
                Diff = findDiff organization.WarunkiPomocy updatedOrg.WarunkiPomocy
                EntityId = id
            }
            transaction.Complete()
        }
let changeBeneficjenci
    (readBy: TeczkaId -> Async<Organization.Organization>)
    (save: Organization.Organization -> Async<unit>)
    (track: AuditTrail -> Async<unit>) : ChangeBeneficjenci =
    fun (id, audit, cmd) ->
        let beneficjenci = {
            Beneficjenci = cmd.Beneficjenci
            LiczbaBeneficjentow = cmd.LiczbaBeneficjentow
        }
        asyncResult {
            let! teczkaId = id |> TeczkaId.create
            let! organization = readBy teczkaId
            use transaction = openTransaction()
            let updatedOrg = { organization with Beneficjenci = beneficjenci }
            do! save updatedOrg
            do! track {
                Who = audit.Who
                OccuredAt = audit.OccuredAt
                Kind = "Beneficjenci"
                Diff = findDiff organization.Beneficjenci updatedOrg.Beneficjenci
                EntityId = id
            }
            transaction.Complete()
        }

let changeDaneAdresowe
    (readBy: TeczkaId -> Async<Organization.Organization>)
    (save: Organization.Organization -> Async<unit>)
    (track: AuditTrail -> Async<unit>) : ChangeDaneAdresowe =
    fun (id, audit, cmd) ->
        let daneAdresowe = {
            AdresPlacowkiTrafiaZywnosc = cmd.AdresPlacowkiTrafiaZywnosc
            AdresRejestrowy = cmd.AdresRejestrowy
            NazwaOrganizacjiPodpisujacejUmowe = cmd.NazwaOrganizacjiPodpisujacejUmowe
            Powiat = cmd.Powiat
            GminaDzielnica = cmd.GminaDzielnica
            NazwaPlacowkiTrafiaZywnosc = cmd.NazwaPlacowkiTrafiaZywnosc
        }
        asyncResult {
            let! teczkaId = id |> TeczkaId.create
            let! organization = readBy teczkaId
            use transaction = openTransaction()
            let updatedOrg = { organization with DaneAdresowe = daneAdresowe }
            do! save updatedOrg
            do! track {
                Who = audit.Who
                OccuredAt = audit.OccuredAt
                Kind = "DaneAdresowe"
                Diff = findDiff organization.DaneAdresowe updatedOrg.DaneAdresowe
                EntityId = id
            }
            transaction.Complete()
        }

let changeAdresyKsiegowosci
    (readBy: TeczkaId -> Async<Organization.Organization>)
    (save: Organization.Organization -> Async<unit>)
    (track: AuditTrail -> Async<unit>) : ChangeAdresyKsiegowosci =
    fun (id, audit, cmd) ->
        let adresyKsiegowosci = {
            KsiegowanieAdres = cmd.KsiegowanieAdres
            NazwaOrganizacjiKsiegowanieDarowizn = cmd.NazwaOrganizacjiKsiegowanieDarowizn
            TelOrganProwadzacegoKsiegowosc = cmd.TelOrganProwadzacegoKsiegowosc
        }
        asyncResult {
            let! teczkaId = id |> TeczkaId.create
            let! organization = readBy teczkaId
            use transaction = openTransaction()
            let updatedOrg = { organization with AdresyKsiegowosci = adresyKsiegowosci }
            do! save updatedOrg
            do! track {
                Who = audit.Who
                OccuredAt = audit.OccuredAt
                Kind = "AdresyKsiegowosci"
                Diff = findDiff organization.AdresyKsiegowosci updatedOrg.AdresyKsiegowosci
                EntityId = id
            }
            transaction.Complete()
        }

let changeZrodlaZywnosci
    (readBy: TeczkaId -> Async<Organization.Organization>)
    (save: Organization.Organization -> Async<unit>)
    (track: AuditTrail -> Async<unit>) : ChangeZrodlaZywnosci =
    fun (id, audit, cmd) ->
        let zrodlaZywnosci = {
            Bazarki = cmd.Bazarki
            FEPZ2024 = cmd.FEPZ2024
            OdbiorKrotkiTermin = cmd.OdbiorKrotkiTermin
            Machfit = cmd.Machfit
            Sieci = cmd.Sieci
            TylkoNaszMagazyn = cmd.TylkoNaszMagazyn }

        asyncResult {
            let! teczkaId = id |> TeczkaId.create
            let! organization = readBy teczkaId
            use transaction = openTransaction()
            let updatedOrg = { organization with ZrodlaZywnosci = zrodlaZywnosci }
            do! save updatedOrg
            do! track {
                Who = audit.Who
                OccuredAt = audit.OccuredAt
                Kind = "DaneAdresowe"
                Diff = findDiff organization.ZrodlaZywnosci updatedOrg.ZrodlaZywnosci
                EntityId = id
            }
            transaction.Complete()
        }


let changeKontakty
    (readBy: TeczkaId -> Async<Organization.Organization>)
    (save: Organization.Organization -> Async<unit>)
    (track: AuditTrail -> Async<unit>) : ChangeKontakty =
    fun (id, audit, cmd) ->
        let kontakty = {
            Email = cmd.Email
            Telefon = cmd.Telefon
            OsobaDoKontaktu = cmd.OsobaDoKontaktu
            MailOsobyKontaktowej = cmd.MailOsobyKontaktowej
            OsobaOdbierajacaZywnosc = cmd.OsobaOdbierajacaZywnosc
            TelefonOsobyOdbierajacej = cmd.TelefonOsobyOdbierajacej
            TelefonOsobyKontaktowej = cmd.TelefonOsobyKontaktowej
            Kontakt = cmd.Kontakt
            Przedstawiciel = cmd.Przedstawiciel
            Dostepnosc = cmd.Dostepnosc
            WwwFacebook = cmd.WwwFacebook
        }
        asyncResult {
            let! teczkaId = id |> TeczkaId.create
            let! organization = readBy teczkaId
            use transaction = openTransaction()
            let updatedOrg = { organization with Kontakty = kontakty }
            do! save updatedOrg
            do! track {
                Who = audit.Who
                OccuredAt = audit.OccuredAt
                Kind = "Kontakty"
                Diff = findDiff organization.Kontakty updatedOrg.Kontakty
                EntityId = id
            }
            transaction.Complete()
        }