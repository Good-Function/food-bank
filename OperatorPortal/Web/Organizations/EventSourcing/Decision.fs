module Organizations.EventSourcing.Decision

open Organizations.Domain.Identifiers
open Organizations.Domain.FormaPrawna
open Organizations.EventSourcing.Commands
open Organizations.EventSourcing.Events
open Organizations.EventSourcing.Aggregate

let private extractKrsNr (formaPrawna: Organizations.Domain.FormaPrawna.FormaPrawna) =
    match formaPrawna.Rejestracja with
    | WRejestrzeKRS krs -> Krs.unwrap krs
    | PozaRejestrem _ -> ""

let decide
    (command: OrganizationCommand)
    (state: OrganizationState option)
    (audit: EventAudit)
    : Result<OrganizationEvent list, string> =

    match command, state with

    | OrganizationCommand.CreateOrganization cmd, None ->
        let event: OrganizationCreatedV1 = {
            TeczkaId = TeczkaId.unwrap cmd.Teczka
            IdentyfikatorEnova = cmd.IdentyfikatorEnova
            NIP = Nip.unwrap cmd.NIP
            Regon = Regon.unwrap cmd.Regon
            KrsNr = extractKrsNr cmd.FormaPrawna
            FormaPrawna = cmd.FormaPrawna.Nazwa
            OPP = cmd.OPP
            DaneAdresowe = {
                TeczkaId = TeczkaId.unwrap cmd.Teczka
                NazwaOrganizacjiPodpisujacejUmowe = cmd.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
                AdresRejestrowy = cmd.DaneAdresowe.AdresRejestrowy
                NazwaPlacowkiTrafiaZywnosc = cmd.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
                AdresPlacowkiTrafiaZywnosc = cmd.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
                GminaDzielnica = cmd.DaneAdresowe.GminaDzielnica
                Powiat = cmd.DaneAdresowe.Powiat
                Audit = audit
            }
            Kontakty = {
                TeczkaId = TeczkaId.unwrap cmd.Teczka
                Email = cmd.Kontakty.Email
                Telefon = cmd.Kontakty.Telefon
                OsobaDoKontaktu = cmd.Kontakty.OsobaDoKontaktu
                TelefonOsobyKontaktowej = cmd.Kontakty.TelefonOsobyKontaktowej
                MailOsobyKontaktowej = cmd.Kontakty.MailOsobyKontaktowej
                OsobaOdbierajacaZywnosc = cmd.Kontakty.OsobaOdbierajacaZywnosc
                TelefonOsobyOdbierajacej = cmd.Kontakty.TelefonOsobyOdbierajacej
                Kontakt = cmd.Kontakty.Kontakt
                Przedstawiciel = cmd.Kontakty.Przedstawiciel
                Dostepnosc = cmd.Kontakty.Dostepnosc
                WwwFacebook = cmd.Kontakty.WwwFacebook
                Audit = audit
            }
            ZrodlaZywnosci = {
                TeczkaId = TeczkaId.unwrap cmd.Teczka
                Bazarki = cmd.ZrodlaZywnosci.Bazarki
                FEPZ2024 = cmd.ZrodlaZywnosci.FEPZ2024
                OdbiorKrotkiTermin = cmd.ZrodlaZywnosci.OdbiorKrotkiTermin
                Machfit = cmd.ZrodlaZywnosci.Machfit
                Sieci = cmd.ZrodlaZywnosci.Sieci
                TylkoNaszMagazyn = cmd.ZrodlaZywnosci.TylkoNaszMagazyn
                Audit = audit
            }
            AdresyKsiegowosci = {
                TeczkaId = TeczkaId.unwrap cmd.Teczka
                KsiegowanieAdres = cmd.AdresyKsiegowosci.KsiegowanieAdres
                NazwaOrganizacjiKsiegowanieDarowizn = cmd.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
                TelOrganProwadzacegoKsiegowosc = cmd.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
                Audit = audit
            }
            Beneficjenci = {
                TeczkaId = TeczkaId.unwrap cmd.Teczka
                LiczbaBeneficjentow = cmd.Beneficjenci.LiczbaBeneficjentow
                Beneficjenci = cmd.Beneficjenci.Beneficjenci
                Audit = audit
            }
            WarunkiPomocy = {
                TeczkaId = TeczkaId.unwrap cmd.Teczka
                Kategoria = cmd.WarunkiPomocy.Kategoria
                RodzajPomocy = cmd.WarunkiPomocy.RodzajPomocy
                SposobUdzielaniaPomocy = cmd.WarunkiPomocy.SposobUdzielaniaPomocy
                WarunkiMagazynowe = cmd.WarunkiPomocy.WarunkiMagazynowe
                HACCP = cmd.WarunkiPomocy.HACCP
                Sanepid = cmd.WarunkiPomocy.Sanepid
                TransportOpis = cmd.WarunkiPomocy.TransportOpis
                TransportKategoria = cmd.WarunkiPomocy.TransportKategoria
                Audit = audit
            }
            Audit = audit
        }
        Ok [ OrganizationEvent.OrganizationCreated event ]

    | OrganizationCommand.CreateOrganization _, Some _ ->
        Error "Organization already exists"

    | OrganizationCommand.ChangeKontakty cmd, Some _ ->
        let event: KontaktyChangedV1 = {
            TeczkaId = TeczkaId.unwrap cmd.TeczkaId
            Email = cmd.Kontakty.Email
            Telefon = cmd.Kontakty.Telefon
            OsobaDoKontaktu = cmd.Kontakty.OsobaDoKontaktu
            TelefonOsobyKontaktowej = cmd.Kontakty.TelefonOsobyKontaktowej
            MailOsobyKontaktowej = cmd.Kontakty.MailOsobyKontaktowej
            OsobaOdbierajacaZywnosc = cmd.Kontakty.OsobaOdbierajacaZywnosc
            TelefonOsobyOdbierajacej = cmd.Kontakty.TelefonOsobyOdbierajacej
            Kontakt = cmd.Kontakty.Kontakt
            Przedstawiciel = cmd.Kontakty.Przedstawiciel
            Dostepnosc = cmd.Kontakty.Dostepnosc
            WwwFacebook = cmd.Kontakty.WwwFacebook
            Audit = audit
        }
        Ok [ OrganizationEvent.KontaktyChanged event ]

    | OrganizationCommand.ChangeKontakty _, None ->
        Error "Organization does not exist"

    | OrganizationCommand.ChangeZrodlaZywnosci cmd, Some _ ->
        let event: ZrodlaZywnosciChangedV1 = {
            TeczkaId = TeczkaId.unwrap cmd.TeczkaId
            Bazarki = cmd.ZrodlaZywnosci.Bazarki
            FEPZ2024 = cmd.ZrodlaZywnosci.FEPZ2024
            OdbiorKrotkiTermin = cmd.ZrodlaZywnosci.OdbiorKrotkiTermin
            Machfit = cmd.ZrodlaZywnosci.Machfit
            Sieci = cmd.ZrodlaZywnosci.Sieci
            TylkoNaszMagazyn = cmd.ZrodlaZywnosci.TylkoNaszMagazyn
            Audit = audit
        }
        Ok [ OrganizationEvent.ZrodlaZywnosciChanged event ]

    | OrganizationCommand.ChangeZrodlaZywnosci _, None ->
        Error "Organization does not exist"

    | OrganizationCommand.ChangeAdresyKsiegowosci cmd, Some _ ->
        let event: AdresyKsiegowosciChangedV1 = {
            TeczkaId = TeczkaId.unwrap cmd.TeczkaId
            KsiegowanieAdres = cmd.AdresyKsiegowosci.KsiegowanieAdres
            NazwaOrganizacjiKsiegowanieDarowizn = cmd.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
            TelOrganProwadzacegoKsiegowosc = cmd.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
            Audit = audit
        }
        Ok [ OrganizationEvent.AdresyKsiegowosciChanged event ]

    | OrganizationCommand.ChangeAdresyKsiegowosci _, None ->
        Error "Organization does not exist"

    | OrganizationCommand.ChangeDaneAdresowe cmd, Some _ ->
        let event: DaneAdresoweChangedV1 = {
            TeczkaId = TeczkaId.unwrap cmd.TeczkaId
            NazwaOrganizacjiPodpisujacejUmowe = cmd.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
            AdresRejestrowy = cmd.DaneAdresowe.AdresRejestrowy
            NazwaPlacowkiTrafiaZywnosc = cmd.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            AdresPlacowkiTrafiaZywnosc = cmd.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
            GminaDzielnica = cmd.DaneAdresowe.GminaDzielnica
            Powiat = cmd.DaneAdresowe.Powiat
            Audit = audit
        }
        Ok [ OrganizationEvent.DaneAdresoweChanged event ]

    | OrganizationCommand.ChangeDaneAdresowe _, None ->
        Error "Organization does not exist"

    | OrganizationCommand.ChangeBeneficjenci cmd, Some _ ->
        let event: BeneficjenciChangedV1 = {
            TeczkaId = TeczkaId.unwrap cmd.TeczkaId
            LiczbaBeneficjentow = cmd.Beneficjenci.LiczbaBeneficjentow
            Beneficjenci = cmd.Beneficjenci.Beneficjenci
            Audit = audit
        }
        Ok [ OrganizationEvent.BeneficjenciChanged event ]

    | OrganizationCommand.ChangeBeneficjenci _, None ->
        Error "Organization does not exist"

    | OrganizationCommand.ChangeWarunkiPomocy cmd, Some _ ->
        let event: WarunkiPomocyChangedV1 = {
            TeczkaId = TeczkaId.unwrap cmd.TeczkaId
            Kategoria = cmd.WarunkiPomocy.Kategoria
            RodzajPomocy = cmd.WarunkiPomocy.RodzajPomocy
            SposobUdzielaniaPomocy = cmd.WarunkiPomocy.SposobUdzielaniaPomocy
            WarunkiMagazynowe = cmd.WarunkiPomocy.WarunkiMagazynowe
            HACCP = cmd.WarunkiPomocy.HACCP
            Sanepid = cmd.WarunkiPomocy.Sanepid
            TransportOpis = cmd.WarunkiPomocy.TransportOpis
            TransportKategoria = cmd.WarunkiPomocy.TransportKategoria
            Audit = audit
        }
        Ok [ OrganizationEvent.WarunkiPomocyChanged event ]

    | OrganizationCommand.ChangeWarunkiPomocy _, None ->
        Error "Organization does not exist"
