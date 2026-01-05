module Organizations.EventSourcing.DecisionTests

open System
open Xunit
open FsUnit.Xunit
open Organizations.Domain.Identifiers
open Organizations.Domain.FormaPrawna
open Organizations.Domain.Organization
open Organizations.EventSourcing.Events
open Organizations.EventSourcing.Commands
open Organizations.EventSourcing.Aggregate
open Organizations.EventSourcing.Decision

let createTestTeczkaId() =
    match TeczkaId.create 123L with
    | Ok id -> id
    | Error _ -> failwith "Failed to create test TeczkaId"

let createTestNip() =
    match Nip.create "1234567890" with
    | Ok nip -> nip
    | Error _ -> failwith "Failed to create test NIP"

let createTestRegon() =
    match Regon.create "123456789" with
    | Ok regon -> regon
    | Error _ -> failwith "Failed to create test Regon"

let createTestKrs() =
    match Krs.create "0000123456" with
    | Ok krs -> krs
    | Error _ -> failwith "Failed to create test KRS"

let testAudit = { Who = "test@example.com"; OccurredAt = DateTime(2026, 1, 4, 10, 0, 0) }

[<Fact>]
let ``CreateOrganization command produces OrganizationCreated event`` () =
    let command = OrganizationCommand.CreateOrganization {
        Teczka = createTestTeczkaId()
        IdentyfikatorEnova = "ENOVA123"
        NIP = createTestNip()
        Regon = createTestRegon()
        FormaPrawna = { Nazwa = "Fundacja"; Rejestracja = WRejestrzeKRS (createTestKrs()) }
        OPP = true
        DaneAdresowe = {
            NazwaOrganizacjiPodpisujacejUmowe = "Test Foundation"
            AdresRejestrowy = "ul. Test 1"
            NazwaPlacowkiTrafiaZywnosc = "Test Point"
            AdresPlacowkiTrafiaZywnosc = "ul. Test 2"
            GminaDzielnica = "Test District"
            Powiat = "Test County"
        }
        Kontakty = {
            Email = "test@test.com"
            Telefon = "123456789"
            OsobaDoKontaktu = "John Doe"
            TelefonOsobyKontaktowej = "987654321"
            MailOsobyKontaktowej = "contact@test.com"
            OsobaOdbierajacaZywnosc = "Jane Doe"
            TelefonOsobyOdbierajacej = "111222333"
            Kontakt = "Email"
            Przedstawiciel = "Representative"
            Dostepnosc = "9-17"
            WwwFacebook = "https://fb.com/test"
        }
        ZrodlaZywnosci = {
            Sieci = true
            Bazarki = false
            Machfit = true
            FEPZ2024 = false
            OdbiorKrotkiTermin = true
            TylkoNaszMagazyn = false
        }
        AdresyKsiegowosci = {
            NazwaOrganizacjiKsiegowanieDarowizn = "Accounting Co."
            KsiegowanieAdres = "ul. Accounting 1"
            TelOrganProwadzacegoKsiegowosc = "123123123"
        }
        Beneficjenci = {
            LiczbaBeneficjentow = 100
            Beneficjenci = "Families in need"
        }
        WarunkiPomocy = {
            Kategoria = "Food aid"
            RodzajPomocy = "Food packages"
            SposobUdzielaniaPomocy = "Direct pickup"
            WarunkiMagazynowe = "Cold storage"
            HACCP = true
            Sanepid = true
            TransportOpis = "Van"
            TransportKategoria = "Own"
        }
    }

    let result = decide command None testAudit

    match result with
    | Ok [ OrganizationCreated event ] ->
        event.TeczkaId |> should equal 123L
        event.IdentyfikatorEnova |> should equal "ENOVA123"
        event.NIP |> should equal "1234567890"
        event.Regon |> should equal "123456789"
        event.FormaPrawna |> should equal "Fundacja"
        event.OPP |> should be True
        event.Audit.Who |> should equal "test@example.com"
    | _ -> Assert.Fail("Expected OrganizationCreated event")

[<Fact>]
let ``CreateOrganization on existing organization returns error`` () =
    let existingState = Some (OrganizationState.empty (createTestTeczkaId()))
    let command = OrganizationCommand.CreateOrganization {
        Teczka = createTestTeczkaId()
        IdentyfikatorEnova = "ENOVA123"
        NIP = createTestNip()
        Regon = createTestRegon()
        FormaPrawna = { Nazwa = "Fundacja"; Rejestracja = PozaRejestrem "" }
        OPP = false
        DaneAdresowe = {
            NazwaOrganizacjiPodpisujacejUmowe = ""
            AdresRejestrowy = ""
            NazwaPlacowkiTrafiaZywnosc = ""
            AdresPlacowkiTrafiaZywnosc = ""
            GminaDzielnica = ""
            Powiat = ""
        }
        Kontakty = {
            Email = ""
            Telefon = ""
            OsobaDoKontaktu = ""
            TelefonOsobyKontaktowej = ""
            MailOsobyKontaktowej = ""
            OsobaOdbierajacaZywnosc = ""
            TelefonOsobyOdbierajacej = ""
            Kontakt = ""
            Przedstawiciel = ""
            Dostepnosc = ""
            WwwFacebook = ""
        }
        ZrodlaZywnosci = {
            Sieci = false
            Bazarki = false
            Machfit = false
            FEPZ2024 = false
            OdbiorKrotkiTermin = false
            TylkoNaszMagazyn = false
        }
        AdresyKsiegowosci = {
            NazwaOrganizacjiKsiegowanieDarowizn = ""
            KsiegowanieAdres = ""
            TelOrganProwadzacegoKsiegowosc = ""
        }
        Beneficjenci = {
            LiczbaBeneficjentow = 0
            Beneficjenci = ""
        }
        WarunkiPomocy = {
            Kategoria = ""
            RodzajPomocy = ""
            SposobUdzielaniaPomocy = ""
            WarunkiMagazynowe = ""
            HACCP = false
            Sanepid = false
            TransportOpis = ""
            TransportKategoria = ""
        }
    }

    let result = decide command existingState testAudit

    match result with
    | Error msg ->
        msg.Contains("already exists") |> should be True
    | Ok _ -> Assert.Fail("Expected error for existing organization")

[<Fact>]
let ``ChangeKontakty produces KontaktyChanged event`` () =
    let existingState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }
    let command = OrganizationCommand.ChangeKontakty {
        TeczkaId = createTestTeczkaId()
        Kontakty = {
            Email = "new@example.com"
            Telefon = "987654321"
            OsobaDoKontaktu = "New Person"
            TelefonOsobyKontaktowej = "111111111"
            MailOsobyKontaktowej = "newcontact@example.com"
            OsobaOdbierajacaZywnosc = "New Receiver"
            TelefonOsobyOdbierajacej = "222222222"
            Kontakt = "Phone"
            Przedstawiciel = "New Rep"
            Dostepnosc = "24/7"
            WwwFacebook = "https://fb.com/new"
        }
    }

    let result = decide command existingState testAudit

    match result with
    | Ok [ KontaktyChanged event ] ->
        event.Email |> should equal "new@example.com"
        event.Telefon |> should equal "987654321"
        event.Audit.Who |> should equal "test@example.com"
    | _ -> Assert.Fail("Expected KontaktyChanged event")

[<Fact>]
let ``ChangeKontakty on non-existent organization returns error`` () =
    let command = OrganizationCommand.ChangeKontakty {
        TeczkaId = createTestTeczkaId()
        Kontakty = {
            Email = ""
            Telefon = ""
            OsobaDoKontaktu = ""
            TelefonOsobyKontaktowej = ""
            MailOsobyKontaktowej = ""
            OsobaOdbierajacaZywnosc = ""
            TelefonOsobyOdbierajacej = ""
            Kontakt = ""
            Przedstawiciel = ""
            Dostepnosc = ""
            WwwFacebook = ""
        }
    }

    let result = decide command None testAudit

    match result with
    | Error msg ->
        msg.Contains("does not exist") |> should be True
    | Ok _ -> Assert.Fail("Expected error for non-existent organization")

[<Fact>]
let ``ChangeZrodlaZywnosci produces ZrodlaZywnosciChanged event`` () =
    let existingState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }
    let command = OrganizationCommand.ChangeZrodlaZywnosci {
        TeczkaId = createTestTeczkaId()
        ZrodlaZywnosci = {
            Sieci = true
            Bazarki = true
            Machfit = false
            FEPZ2024 = true
            OdbiorKrotkiTermin = false
            TylkoNaszMagazyn = true
        }
    }

    let result = decide command existingState testAudit

    match result with
    | Ok [ ZrodlaZywnosciChanged event ] ->
        event.Sieci |> should be True
        event.Bazarki |> should be True
        event.FEPZ2024 |> should be True
    | _ -> Assert.Fail("Expected ZrodlaZywnosciChanged event")

[<Fact>]
let ``ChangeAdresyKsiegowosci produces AdresyKsiegowosciChanged event`` () =
    let existingState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }
    let command = OrganizationCommand.ChangeAdresyKsiegowosci {
        TeczkaId = createTestTeczkaId()
        AdresyKsiegowosci = {
            NazwaOrganizacjiKsiegowanieDarowizn = "New Accounting"
            KsiegowanieAdres = "New Address"
            TelOrganProwadzacegoKsiegowosc = "999888777"
        }
    }

    let result = decide command existingState testAudit

    match result with
    | Ok [ AdresyKsiegowosciChanged event ] ->
        event.NazwaOrganizacjiKsiegowanieDarowizn |> should equal "New Accounting"
        event.KsiegowanieAdres |> should equal "New Address"
    | _ -> Assert.Fail("Expected AdresyKsiegowosciChanged event")

[<Fact>]
let ``ChangeDaneAdresowe produces DaneAdresoweChanged event`` () =
    let existingState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }
    let command = OrganizationCommand.ChangeDaneAdresowe {
        TeczkaId = createTestTeczkaId()
        DaneAdresowe = {
            NazwaOrganizacjiPodpisujacejUmowe = "New Org Name"
            AdresRejestrowy = "New Registry Address"
            NazwaPlacowkiTrafiaZywnosc = "New Point Name"
            AdresPlacowkiTrafiaZywnosc = "New Point Address"
            GminaDzielnica = "New District"
            Powiat = "New County"
        }
    }

    let result = decide command existingState testAudit

    match result with
    | Ok [ DaneAdresoweChanged event ] ->
        event.NazwaOrganizacjiPodpisujacejUmowe |> should equal "New Org Name"
        event.GminaDzielnica |> should equal "New District"
    | _ -> Assert.Fail("Expected DaneAdresoweChanged event")

[<Fact>]
let ``ChangeBeneficjenci produces BeneficjenciChanged event`` () =
    let existingState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }
    let command = OrganizationCommand.ChangeBeneficjenci {
        TeczkaId = createTestTeczkaId()
        Beneficjenci = {
            LiczbaBeneficjentow = 250
            Beneficjenci = "New beneficiary description"
        }
    }

    let result = decide command existingState testAudit

    match result with
    | Ok [ BeneficjenciChanged event ] ->
        event.LiczbaBeneficjentow |> should equal 250
        event.Beneficjenci |> should equal "New beneficiary description"
    | _ -> Assert.Fail("Expected BeneficjenciChanged event")

[<Fact>]
let ``ChangeWarunkiPomocy produces WarunkiPomocyChanged event`` () =
    let existingState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }
    let command = OrganizationCommand.ChangeWarunkiPomocy {
        TeczkaId = createTestTeczkaId()
        WarunkiPomocy = {
            Kategoria = "New Category"
            RodzajPomocy = "New Aid Type"
            SposobUdzielaniaPomocy = "New Distribution Method"
            WarunkiMagazynowe = "New Storage Conditions"
            HACCP = false
            Sanepid = false
            TransportOpis = "New Transport Description"
            TransportKategoria = "New Transport Category"
        }
    }

    let result = decide command existingState testAudit

    match result with
    | Ok [ WarunkiPomocyChanged event ] ->
        event.Kategoria |> should equal "New Category"
        event.HACCP |> should be False
    | _ -> Assert.Fail("Expected WarunkiPomocyChanged event")
