module Organizations.EventSourcing.EvolutionTests

open System
open Xunit
open FsUnit.Xunit
open Organizations.Domain.Identifiers
open Organizations.EventSourcing.Events
open Organizations.EventSourcing.Aggregate
open Organizations.EventSourcing.Evolution

let createTestTeczkaId() =
    match TeczkaId.create 123L with
    | Ok id -> id
    | Error _ -> failwith "Failed to create test TeczkaId"

let testAudit = { Who = "test@example.com"; OccurredAt = DateTime(2026, 1, 4, 10, 0, 0) }

let createTestOrganizationCreatedEvent () =
    OrganizationCreated {
        TeczkaId = 123L
        IdentyfikatorEnova = "ENOVA123"
        NIP = "1234567890"
        Regon = "123456789"
        KrsNr = "0000123456"
        FormaPrawna = "Fundacja"
        OPP = true
        DaneAdresowe = {
            TeczkaId = 123L
            NazwaOrganizacjiPodpisujacejUmowe = "Test Foundation"
            AdresRejestrowy = "ul. Test 1"
            NazwaPlacowkiTrafiaZywnosc = "Test Point"
            AdresPlacowkiTrafiaZywnosc = "ul. Test 2"
            GminaDzielnica = "Test District"
            Powiat = "Test County"
            Audit = testAudit
        }
        Kontakty = {
            TeczkaId = 123L
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
            Audit = testAudit
        }
        ZrodlaZywnosci = {
            TeczkaId = 123L
            Bazarki = true
            FEPZ2024 = false
            OdbiorKrotkiTermin = true
            Machfit = false
            Sieci = true
            TylkoNaszMagazyn = false
            Audit = testAudit
        }
        AdresyKsiegowosci = {
            TeczkaId = 123L
            KsiegowanieAdres = "ul. Accounting 1"
            NazwaOrganizacjiKsiegowanieDarowizn = "Accounting Co."
            TelOrganProwadzacegoKsiegowosc = "123123123"
            Audit = testAudit
        }
        Beneficjenci = {
            TeczkaId = 123L
            LiczbaBeneficjentow = 100
            Beneficjenci = "Families in need"
            Audit = testAudit
        }
        WarunkiPomocy = {
            TeczkaId = 123L
            Kategoria = "Food aid"
            RodzajPomocy = "Food packages"
            SposobUdzielaniaPomocy = "Direct pickup"
            WarunkiMagazynowe = "Cold storage"
            HACCP = true
            Sanepid = true
            TransportOpis = "Van"
            TransportKategoria = "Own"
            Audit = testAudit
        }
        Audit = testAudit
    }

[<Fact>]
let ``OrganizationCreated event creates new state with version 1`` () =
    let event = createTestOrganizationCreatedEvent()

    let newState = evolve None event

    newState.Version |> should equal 1
    TeczkaId.unwrap newState.Teczka |> should equal 123L
    newState.IdentyfikatorEnova |> should equal "ENOVA123"
    Nip.unwrap newState.NIP |> should equal "1234567890"
    newState.OPP |> should be True

[<Fact>]
let ``KontaktyChanged event updates Kontakty section and increments version`` () =
    let initialState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with
            Version = 1
            Kontakty = {
                Email = "old@example.com"
                Telefon = "000000000"
                OsobaDoKontaktu = "Old Person"
                TelefonOsobyKontaktowej = "111111111"
                MailOsobyKontaktowej = "old@old.com"
                OsobaOdbierajacaZywnosc = "Old Receiver"
                TelefonOsobyOdbierajacej = "222222222"
                Kontakt = "Old Contact"
                Przedstawiciel = "Old Rep"
                Dostepnosc = "Old Availability"
                WwwFacebook = "Old Facebook"
            }
    }

    let event = KontaktyChanged {
        TeczkaId = 123L
        Email = "new@example.com"
        Telefon = "999999999"
        OsobaDoKontaktu = "New Person"
        TelefonOsobyKontaktowej = "888888888"
        MailOsobyKontaktowej = "new@new.com"
        OsobaOdbierajacaZywnosc = "New Receiver"
        TelefonOsobyOdbierajacej = "777777777"
        Kontakt = "New Contact"
        Przedstawiciel = "New Rep"
        Dostepnosc = "New Availability"
        WwwFacebook = "New Facebook"
        Audit = testAudit
    }

    let newState = evolve initialState event

    newState.Kontakty.Email |> should equal "new@example.com"
    newState.Kontakty.Telefon |> should equal "999999999"
    newState.Version |> should equal 2

[<Fact>]
let ``ZrodlaZywnosciChanged event updates ZrodlaZywnosci section`` () =
    let initialState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }

    let event = ZrodlaZywnosciChanged {
        TeczkaId = 123L
        Bazarki = true
        FEPZ2024 = true
        OdbiorKrotkiTermin = false
        Machfit = true
        Sieci = false
        TylkoNaszMagazyn = true
        Audit = testAudit
    }

    let newState = evolve initialState event

    newState.ZrodlaZywnosci.Bazarki |> should be True
    newState.ZrodlaZywnosci.FEPZ2024 |> should be True
    newState.ZrodlaZywnosci.Machfit |> should be True
    newState.Version |> should equal 2

[<Fact>]
let ``AdresyKsiegowosciChanged event updates AdresyKsiegowosci section`` () =
    let initialState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }

    let event = AdresyKsiegowosciChanged {
        TeczkaId = 123L
        KsiegowanieAdres = "New Accounting Address"
        NazwaOrganizacjiKsiegowanieDarowizn = "New Accounting Org"
        TelOrganProwadzacegoKsiegowosc = "555555555"
        Audit = testAudit
    }

    let newState = evolve initialState event

    newState.AdresyKsiegowosci.KsiegowanieAdres |> should equal "New Accounting Address"
    newState.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn |> should equal "New Accounting Org"
    newState.Version |> should equal 2

[<Fact>]
let ``DaneAdresoweChanged event updates DaneAdresowe section`` () =
    let initialState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }

    let event = DaneAdresoweChanged {
        TeczkaId = 123L
        NazwaOrganizacjiPodpisujacejUmowe = "New Organization Name"
        AdresRejestrowy = "New Registry Address"
        NazwaPlacowkiTrafiaZywnosc = "New Point Name"
        AdresPlacowkiTrafiaZywnosc = "New Point Address"
        GminaDzielnica = "New District"
        Powiat = "New County"
        Audit = testAudit
    }

    let newState = evolve initialState event

    newState.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe |> should equal "New Organization Name"
    newState.DaneAdresowe.GminaDzielnica |> should equal "New District"
    newState.Version |> should equal 2

[<Fact>]
let ``BeneficjenciChanged event updates Beneficjenci section`` () =
    let initialState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }

    let event = BeneficjenciChanged {
        TeczkaId = 123L
        LiczbaBeneficjentow = 250
        Beneficjenci = "New beneficiaries description"
        Audit = testAudit
    }

    let newState = evolve initialState event

    newState.Beneficjenci.LiczbaBeneficjentow |> should equal 250
    newState.Beneficjenci.Beneficjenci |> should equal "New beneficiaries description"
    newState.Version |> should equal 2

[<Fact>]
let ``WarunkiPomocyChanged event updates WarunkiPomocy section`` () =
    let initialState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 1
    }

    let event = WarunkiPomocyChanged {
        TeczkaId = 123L
        Kategoria = "New Category"
        RodzajPomocy = "New Aid Type"
        SposobUdzielaniaPomocy = "New Method"
        WarunkiMagazynowe = "New Storage"
        HACCP = false
        Sanepid = false
        TransportOpis = "New Transport"
        TransportKategoria = "New Transport Category"
        Audit = testAudit
    }

    let newState = evolve initialState event

    newState.WarunkiPomocy.Kategoria |> should equal "New Category"
    newState.WarunkiPomocy.HACCP |> should be False
    newState.Version |> should equal 2

[<Fact>]
let ``evolve increments version correctly`` () =
    let initialState = Some {
        OrganizationState.empty (createTestTeczkaId())
        with Version = 5
    }

    let event = KontaktyChanged {
        TeczkaId = 123L
        Email = "test@test.com"
        Telefon = "123456789"
        OsobaDoKontaktu = "Person"
        TelefonOsobyKontaktowej = "987654321"
        MailOsobyKontaktowej = "mail@test.com"
        OsobaOdbierajacaZywnosc = "Receiver"
        TelefonOsobyOdbierajacej = "111222333"
        Kontakt = "Contact"
        Przedstawiciel = "Rep"
        Dostepnosc = "Avail"
        WwwFacebook = "FB"
        Audit = testAudit
    }

    let newState = evolve initialState event

    newState.Version |> should equal 6

[<Fact>]
let ``replay empty list returns None`` () =
    let result = replay []

    result |> should equal None

[<Fact>]
let ``replay single OrganizationCreated event returns state with version 1`` () =
    let events = [ createTestOrganizationCreatedEvent() ]

    let result = replay events

    match result with
    | Some state ->
        state.Version |> should equal 1
        TeczkaId.unwrap state.Teczka |> should equal 123L
    | None -> Assert.Fail("Expected Some state")

[<Fact>]
let ``replay multiple events applies them sequentially`` () =
    let events = [
        createTestOrganizationCreatedEvent()
        KontaktyChanged {
            TeczkaId = 123L
            Email = "updated@test.com"
            Telefon = "999888777"
            OsobaDoKontaktu = "Updated Person"
            TelefonOsobyKontaktowej = "666555444"
            MailOsobyKontaktowej = "updated@mail.com"
            OsobaOdbierajacaZywnosc = "Updated Receiver"
            TelefonOsobyOdbierajacej = "333222111"
            Kontakt = "Updated Contact"
            Przedstawiciel = "Updated Rep"
            Dostepnosc = "Updated Availability"
            WwwFacebook = "Updated FB"
            Audit = testAudit
        }
        ZrodlaZywnosciChanged {
            TeczkaId = 123L
            Bazarki = false
            FEPZ2024 = true
            OdbiorKrotkiTermin = true
            Machfit = true
            Sieci = false
            TylkoNaszMagazyn = true
            Audit = testAudit
        }
    ]

    let result = replay events

    match result with
    | Some state ->
        state.Version |> should equal 3
        state.Kontakty.Email |> should equal "updated@test.com"
        state.ZrodlaZywnosci.FEPZ2024 |> should be True
        state.ZrodlaZywnosci.Machfit |> should be True
    | None -> Assert.Fail("Expected Some state")
