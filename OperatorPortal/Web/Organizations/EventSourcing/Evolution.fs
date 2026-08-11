module Organizations.EventSourcing.Evolution

open Organizations.Domain.Identifiers
open Organizations.Domain.FormaPrawna
open Organizations.EventSourcing.Events
open Organizations.EventSourcing.Aggregate

let private createFormaPrawna (formaPrawnaName: string) (krsNr: string) =
    if krsNr = "" then
        { Nazwa = formaPrawnaName; Rejestracja = PozaRejestrem "" }
    else
        match Krs.create krsNr with
        | Ok krs -> { Nazwa = formaPrawnaName; Rejestracja = WRejestrzeKRS krs }
        | Error _ -> { Nazwa = formaPrawnaName; Rejestracja = PozaRejestrem krsNr }

let evolve
    (state: OrganizationState option)
    (event: OrganizationEvent)
    : OrganizationState =

    match event, state with

    | OrganizationCreated e, None ->
        let teczkaId =
            match TeczkaId.create e.TeczkaId with
            | Ok id -> id
            | Error _ -> failwith $"Invalid TeczkaId in event: {e.TeczkaId}"

        let nip =
            match Nip.create e.NIP with
            | Ok n -> n
            | Error _ -> failwith $"Invalid NIP in event: {e.NIP}"

        let regon =
            match Regon.create e.Regon with
            | Ok r -> r
            | Error _ -> failwith $"Invalid Regon in event: {e.Regon}"

        {
            Teczka = teczkaId
            IdentyfikatorEnova = e.IdentyfikatorEnova
            NIP = nip
            Regon = regon
            FormaPrawna = createFormaPrawna e.FormaPrawna e.KrsNr
            OPP = e.OPP
            DaneAdresowe = {
                NazwaOrganizacjiPodpisujacejUmowe = e.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
                AdresRejestrowy = e.DaneAdresowe.AdresRejestrowy
                NazwaPlacowkiTrafiaZywnosc = e.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
                AdresPlacowkiTrafiaZywnosc = e.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
                GminaDzielnica = e.DaneAdresowe.GminaDzielnica
                Powiat = e.DaneAdresowe.Powiat
            }
            Kontakty = {
                Email = e.Kontakty.Email
                Telefon = e.Kontakty.Telefon
                OsobaDoKontaktu = e.Kontakty.OsobaDoKontaktu
                TelefonOsobyKontaktowej = e.Kontakty.TelefonOsobyKontaktowej
                MailOsobyKontaktowej = e.Kontakty.MailOsobyKontaktowej
                OsobaOdbierajacaZywnosc = e.Kontakty.OsobaOdbierajacaZywnosc
                TelefonOsobyOdbierajacej = e.Kontakty.TelefonOsobyOdbierajacej
                Kontakt = e.Kontakty.Kontakt
                Przedstawiciel = e.Kontakty.Przedstawiciel
                Dostepnosc = e.Kontakty.Dostepnosc
                WwwFacebook = e.Kontakty.WwwFacebook
            }
            ZrodlaZywnosci = {
                Sieci = e.ZrodlaZywnosci.Sieci
                Bazarki = e.ZrodlaZywnosci.Bazarki
                Machfit = e.ZrodlaZywnosci.Machfit
                FEPZ2024 = e.ZrodlaZywnosci.FEPZ2024
                OdbiorKrotkiTermin = e.ZrodlaZywnosci.OdbiorKrotkiTermin
                TylkoNaszMagazyn = e.ZrodlaZywnosci.TylkoNaszMagazyn
            }
            AdresyKsiegowosci = {
                KsiegowanieAdres = e.AdresyKsiegowosci.KsiegowanieAdres
                NazwaOrganizacjiKsiegowanieDarowizn = e.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
                TelOrganProwadzacegoKsiegowosc = e.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
            }
            Beneficjenci = {
                LiczbaBeneficjentow = e.Beneficjenci.LiczbaBeneficjentow
                Beneficjenci = e.Beneficjenci.Beneficjenci
            }
            WarunkiPomocy = {
                Kategoria = e.WarunkiPomocy.Kategoria
                RodzajPomocy = e.WarunkiPomocy.RodzajPomocy
                SposobUdzielaniaPomocy = e.WarunkiPomocy.SposobUdzielaniaPomocy
                WarunkiMagazynowe = e.WarunkiPomocy.WarunkiMagazynowe
                HACCP = e.WarunkiPomocy.HACCP
                Sanepid = e.WarunkiPomocy.Sanepid
                TransportOpis = e.WarunkiPomocy.TransportOpis
                TransportKategoria = e.WarunkiPomocy.TransportKategoria
            }
            Version = 1
        }

    | KontaktyChanged e, Some org ->
        { org with
            Kontakty = {
                Email = e.Email
                Telefon = e.Telefon
                OsobaDoKontaktu = e.OsobaDoKontaktu
                TelefonOsobyKontaktowej = e.TelefonOsobyKontaktowej
                MailOsobyKontaktowej = e.MailOsobyKontaktowej
                OsobaOdbierajacaZywnosc = e.OsobaOdbierajacaZywnosc
                TelefonOsobyOdbierajacej = e.TelefonOsobyOdbierajacej
                Kontakt = e.Kontakt
                Przedstawiciel = e.Przedstawiciel
                Dostepnosc = e.Dostepnosc
                WwwFacebook = e.WwwFacebook
            }
            Version = org.Version + 1
        }

    | ZrodlaZywnosciChanged e, Some org ->
        { org with
            ZrodlaZywnosci = {
                Sieci = e.Sieci
                Bazarki = e.Bazarki
                Machfit = e.Machfit
                FEPZ2024 = e.FEPZ2024
                OdbiorKrotkiTermin = e.OdbiorKrotkiTermin
                TylkoNaszMagazyn = e.TylkoNaszMagazyn
            }
            Version = org.Version + 1
        }

    | AdresyKsiegowosciChanged e, Some org ->
        { org with
            AdresyKsiegowosci = {
                KsiegowanieAdres = e.KsiegowanieAdres
                NazwaOrganizacjiKsiegowanieDarowizn = e.NazwaOrganizacjiKsiegowanieDarowizn
                TelOrganProwadzacegoKsiegowosc = e.TelOrganProwadzacegoKsiegowosc
            }
            Version = org.Version + 1
        }

    | DaneAdresoweChanged e, Some org ->
        { org with
            DaneAdresowe = {
                NazwaOrganizacjiPodpisujacejUmowe = e.NazwaOrganizacjiPodpisujacejUmowe
                AdresRejestrowy = e.AdresRejestrowy
                NazwaPlacowkiTrafiaZywnosc = e.NazwaPlacowkiTrafiaZywnosc
                AdresPlacowkiTrafiaZywnosc = e.AdresPlacowkiTrafiaZywnosc
                GminaDzielnica = e.GminaDzielnica
                Powiat = e.Powiat
            }
            Version = org.Version + 1
        }

    | BeneficjenciChanged e, Some org ->
        { org with
            Beneficjenci = {
                LiczbaBeneficjentow = e.LiczbaBeneficjentow
                Beneficjenci = e.Beneficjenci
            }
            Version = org.Version + 1
        }

    | WarunkiPomocyChanged e, Some org ->
        { org with
            WarunkiPomocy = {
                Kategoria = e.Kategoria
                RodzajPomocy = e.RodzajPomocy
                SposobUdzielaniaPomocy = e.SposobUdzielaniaPomocy
                WarunkiMagazynowe = e.WarunkiMagazynowe
                HACCP = e.HACCP
                Sanepid = e.Sanepid
                TransportOpis = e.TransportOpis
                TransportKategoria = e.TransportKategoria
            }
            Version = org.Version + 1
        }

    | OrganizationCreated _, Some _ ->
        failwith "Cannot create organization that already exists"

    | _, None ->
        failwith "Cannot apply event to non-existent organization"

let replay (events: OrganizationEvent list) : OrganizationState option =
    match events with
    | [] -> None
    | _ ->
        events
        |> List.fold (fun state event -> Some (evolve state event)) None
