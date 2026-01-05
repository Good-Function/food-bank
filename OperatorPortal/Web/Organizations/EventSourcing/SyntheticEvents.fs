module Organizations.EventSourcing.SyntheticEvents

open System
open Organizations.Domain.Identifiers
open Organizations.Domain.FormaPrawna
open Organizations.Domain.Organization
open Organizations.EventSourcing.Events

let createOrganizationCreatedEvent
    (org: Organization)
    (who: string)
    : OrganizationCreatedV1 =
    let audit = { Who = who; OccurredAt = DateTime.UtcNow }

    let krsNr =
        match org.FormaPrawna.Rejestracja with
        | WRejestrzeKRS krs -> Krs.unwrap krs
        | PozaRejestrem _ -> ""

    {
        TeczkaId = TeczkaId.unwrap org.Teczka
        IdentyfikatorEnova = org.IdentyfikatorEnova
        NIP = Nip.unwrap org.NIP
        Regon = Regon.unwrap org.Regon
        KrsNr = krsNr
        FormaPrawna = org.FormaPrawna.Nazwa
        OPP = org.OPP
        DaneAdresowe = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            NazwaOrganizacjiPodpisujacejUmowe = org.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
            AdresRejestrowy = org.DaneAdresowe.AdresRejestrowy
            NazwaPlacowkiTrafiaZywnosc = org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            AdresPlacowkiTrafiaZywnosc = org.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
            GminaDzielnica = org.DaneAdresowe.GminaDzielnica
            Powiat = org.DaneAdresowe.Powiat
            Audit = audit
        }
        Kontakty = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            Email = org.Kontakty.Email
            Telefon = org.Kontakty.Telefon
            OsobaDoKontaktu = org.Kontakty.OsobaDoKontaktu
            TelefonOsobyKontaktowej = org.Kontakty.TelefonOsobyKontaktowej
            MailOsobyKontaktowej = org.Kontakty.MailOsobyKontaktowej
            OsobaOdbierajacaZywnosc = org.Kontakty.OsobaOdbierajacaZywnosc
            TelefonOsobyOdbierajacej = org.Kontakty.TelefonOsobyOdbierajacej
            Kontakt = org.Kontakty.Kontakt
            Przedstawiciel = org.Kontakty.Przedstawiciel
            Dostepnosc = org.Kontakty.Dostepnosc
            WwwFacebook = org.Kontakty.WwwFacebook
            Audit = audit
        }
        ZrodlaZywnosci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            Sieci = org.ZrodlaZywnosci.Sieci
            Bazarki = org.ZrodlaZywnosci.Bazarki
            Machfit = org.ZrodlaZywnosci.Machfit
            FEPZ2024 = org.ZrodlaZywnosci.FEPZ2024
            OdbiorKrotkiTermin = org.ZrodlaZywnosci.OdbiorKrotkiTermin
            TylkoNaszMagazyn = org.ZrodlaZywnosci.TylkoNaszMagazyn
            Audit = audit
        }
        AdresyKsiegowosci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            KsiegowanieAdres = org.AdresyKsiegowosci.KsiegowanieAdres
            NazwaOrganizacjiKsiegowanieDarowizn = org.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
            TelOrganProwadzacegoKsiegowosc = org.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
            Audit = audit
        }
        Beneficjenci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            LiczbaBeneficjentow = org.Beneficjenci.LiczbaBeneficjentow
            Beneficjenci = org.Beneficjenci.Beneficjenci
            Audit = audit
        }
        WarunkiPomocy = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            Kategoria = org.WarunkiPomocy.Kategoria
            RodzajPomocy = org.WarunkiPomocy.RodzajPomocy
            SposobUdzielaniaPomocy = org.WarunkiPomocy.SposobUdzielaniaPomocy
            WarunkiMagazynowe = org.WarunkiPomocy.WarunkiMagazynowe
            HACCP = org.WarunkiPomocy.HACCP
            Sanepid = org.WarunkiPomocy.Sanepid
            TransportOpis = org.WarunkiPomocy.TransportOpis
            TransportKategoria = org.WarunkiPomocy.TransportKategoria
            Audit = audit
        }
        Audit = audit
    }
