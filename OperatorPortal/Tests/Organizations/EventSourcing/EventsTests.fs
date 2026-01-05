module Organizations.EventSourcing.EventsTests

open System
open Xunit
open FsUnit.Xunit
open Organizations.EventSourcing.Events
open EventStore.Serialization

[<Fact>]
let ``KontaktyChangedV1 serializes and deserializes correctly`` () =
    let event = {
        TeczkaId = 123L
        Email = "test@example.com"
        Telefon = "123456789"
        OsobaDoKontaktu = "Jan Kowalski"
        TelefonOsobyKontaktowej = "987654321"
        MailOsobyKontaktowej = "kontakt@example.com"
        OsobaOdbierajacaZywnosc = "Anna Nowak"
        TelefonOsobyOdbierajacej = "111222333"
        Kontakt = "Email preferred"
        Przedstawiciel = "Piotr Wiśniewski"
        Dostepnosc = "9-17"
        WwwFacebook = "https://facebook.com/org"
        Audit = { Who = "user@example.com"; OccurredAt = DateTime(2026, 1, 4, 10, 30, 0, DateTimeKind.Utc) }
    }

    let json = serialize event
    let deserialized = deserialize<KontaktyChangedV1> json

    deserialized.Email |> should equal event.Email
    deserialized.Telefon |> should equal event.Telefon
    deserialized.Audit.Who |> should equal event.Audit.Who

[<Fact>]
let ``ZrodlaZywnosciChangedV1 contains all expected fields`` () =
    let event = {
        TeczkaId = 456L
        Bazarki = true
        FEPZ2024 = false
        OdbiorKrotkiTermin = true
        Machfit = false
        Sieci = true
        TylkoNaszMagazyn = false
        Audit = { Who = "admin@example.com"; OccurredAt = DateTime(2026, 1, 4, 11, 0, 0) }
    }

    let json = serialize event
    let deserialized = deserialize<ZrodlaZywnosciChangedV1> json

    deserialized.TeczkaId |> should equal 456L
    deserialized.Bazarki |> should be True
    deserialized.FEPZ2024 |> should be False
    deserialized.OdbiorKrotkiTermin |> should be True
    deserialized.Machfit |> should be False
    deserialized.Sieci |> should be True
    deserialized.TylkoNaszMagazyn |> should be False
    deserialized.Audit.Who |> should equal "admin@example.com"

[<Fact>]
let ``AdresyKsiegowosciChangedV1 serializes correctly`` () =
    let event = {
        TeczkaId = 789L
        KsiegowanieAdres = "ul. Księgowa 1, Warszawa"
        NazwaOrganizacjiKsiegowanieDarowizn = "Księgowość Sp. z o.o."
        TelOrganProwadzacegoKsiegowosc = "+48 22 123 45 67"
        Audit = { Who = "user@test.com"; OccurredAt = DateTime(2026, 1, 4, 12, 0, 0, DateTimeKind.Utc) }
    }

    let json = serialize event
    let deserialized = deserialize<AdresyKsiegowosciChangedV1> json

    deserialized.KsiegowanieAdres |> should equal event.KsiegowanieAdres
    deserialized.Audit.Who |> should equal event.Audit.Who

[<Fact>]
let ``DaneAdresoweChangedV1 serializes correctly`` () =
    let event = {
        TeczkaId = 101L
        AdresPlacowkiTrafiaZywnosc = "ul. Główna 10, Kraków"
        AdresRejestrowy = "ul. Rejestrowa 5, Warszawa"
        NazwaOrganizacjiPodpisujacejUmowe = "Fundacja Pomocy"
        Powiat = "Krakowski"
        GminaDzielnica = "Śródmieście"
        NazwaPlacowkiTrafiaZywnosc = "Punkt Wydawania Żywności"
        Audit = { Who = "editor@example.com"; OccurredAt = DateTime(2026, 1, 4, 13, 0, 0, DateTimeKind.Utc) }
    }

    let json = serialize event
    let deserialized = deserialize<DaneAdresoweChangedV1> json

    deserialized.NazwaOrganizacjiPodpisujacejUmowe |> should equal event.NazwaOrganizacjiPodpisujacejUmowe
    deserialized.Audit.Who |> should equal event.Audit.Who

[<Fact>]
let ``BeneficjenciChangedV1 serializes correctly`` () =
    let event = {
        TeczkaId = 202L
        Beneficjenci = "Rodziny w trudnej sytuacji życiowej"
        LiczbaBeneficjentow = 150
        Audit = { Who = "moderator@example.com"; OccurredAt = DateTime(2026, 1, 4, 14, 0, 0, DateTimeKind.Utc) }
    }

    let json = serialize event
    let deserialized = deserialize<BeneficjenciChangedV1> json

    deserialized.LiczbaBeneficjentow |> should equal event.LiczbaBeneficjentow
    deserialized.Audit.Who |> should equal event.Audit.Who

[<Fact>]
let ``WarunkiPomocyChangedV1 serializes correctly`` () =
    let event = {
        TeczkaId = 303L
        HACCP = true
        Kategoria = "Pomoc żywnościowa"
        RodzajPomocy = "Paczki żywnościowe"
        Sanepid = true
        SposobUdzielaniaPomocy = "Bezpośredni odbiór"
        TransportKategoria = "Własny"
        TransportOpis = "Samochód dostawczy"
        WarunkiMagazynowe = "Chłodnia i magazyn suchy"
        Audit = { Who = "supervisor@example.com"; OccurredAt = DateTime(2026, 1, 4, 15, 0, 0, DateTimeKind.Utc) }
    }

    let json = serialize event
    let deserialized = deserialize<WarunkiPomocyChangedV1> json

    deserialized.HACCP |> should be True
    deserialized.Kategoria |> should equal event.Kategoria
    deserialized.Audit.Who |> should equal event.Audit.Who

[<Fact>]
let ``OrganizationCreatedV1 captures complete organization state`` () =
    let event = {
        TeczkaId = 999L
        IdentyfikatorEnova = "ENOVA123"
        NIP = "1234567890"
        Regon = "123456789"
        KrsNr = "0000123456"
        FormaPrawna = "Fundacja"
        OPP = true
        DaneAdresowe = {
            TeczkaId = 999L
            NazwaOrganizacjiPodpisujacejUmowe = "Fundacja Test"
            AdresRejestrowy = "ul. Testowa 1"
            NazwaPlacowkiTrafiaZywnosc = "Punkt Testowy"
            AdresPlacowkiTrafiaZywnosc = "ul. Testowa 2"
            GminaDzielnica = "Testowa"
            Powiat = "Testowy"
            Audit = { Who = "system"; OccurredAt = DateTime(2026, 1, 1, 0, 0, 0) }
        }
        Kontakty = {
            TeczkaId = 999L
            Email = "test@test.com"
            Telefon = "123456789"
            OsobaDoKontaktu = "Test Person"
            TelefonOsobyKontaktowej = "987654321"
            MailOsobyKontaktowej = "contact@test.com"
            OsobaOdbierajacaZywnosc = "Receiver"
            TelefonOsobyOdbierajacej = "111222333"
            Kontakt = "Email"
            Przedstawiciel = "Representative"
            Dostepnosc = "24/7"
            WwwFacebook = "https://fb.com/test"
            Audit = { Who = "system"; OccurredAt = DateTime(2026, 1, 1, 0, 0, 0) }
        }
        ZrodlaZywnosci = {
            TeczkaId = 999L
            Bazarki = true
            FEPZ2024 = true
            OdbiorKrotkiTermin = false
            Machfit = false
            Sieci = true
            TylkoNaszMagazyn = false
            Audit = { Who = "system"; OccurredAt = DateTime(2026, 1, 1, 0, 0, 0) }
        }
        AdresyKsiegowosci = {
            TeczkaId = 999L
            KsiegowanieAdres = "ul. Księgowa 1"
            NazwaOrganizacjiKsiegowanieDarowizn = "Księgowość Test"
            TelOrganProwadzacegoKsiegowosc = "123123123"
            Audit = { Who = "system"; OccurredAt = DateTime(2026, 1, 1, 0, 0, 0) }
        }
        Beneficjenci = {
            TeczkaId = 999L
            LiczbaBeneficjentow = 100
            Beneficjenci = "Test beneficiaries"
            Audit = { Who = "system"; OccurredAt = DateTime(2026, 1, 1, 0, 0, 0) }
        }
        WarunkiPomocy = {
            TeczkaId = 999L
            Kategoria = "Test"
            RodzajPomocy = "Test Aid"
            SposobUdzielaniaPomocy = "Direct"
            WarunkiMagazynowe = "Cold storage"
            HACCP = true
            Sanepid = true
            TransportOpis = "Van"
            TransportKategoria = "Own"
            Audit = { Who = "system"; OccurredAt = DateTime(2026, 1, 1, 0, 0, 0) }
        }
        Audit = { Who = "system"; OccurredAt = DateTime(2026, 1, 1, 0, 0, 0) }
    }

    let json = serialize event
    let deserialized = deserialize<OrganizationCreatedV1> json

    deserialized.TeczkaId |> should equal 999L
    deserialized.IdentyfikatorEnova |> should equal "ENOVA123"
    deserialized.NIP |> should equal "1234567890"
    deserialized.Regon |> should equal "123456789"
    deserialized.KrsNr |> should equal "0000123456"
    deserialized.FormaPrawna |> should equal "Fundacja"
    deserialized.OPP |> should be True
    deserialized.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe |> should equal "Fundacja Test"
    deserialized.Kontakty.Email |> should equal "test@test.com"
    deserialized.ZrodlaZywnosci.Sieci |> should be True
    deserialized.AdresyKsiegowosci.KsiegowanieAdres |> should equal "ul. Księgowa 1"
    deserialized.Beneficjenci.LiczbaBeneficjentow |> should equal 100
    deserialized.WarunkiPomocy.HACCP |> should be True
