module Organizations.EventSourcing.Aggregate

open Organizations.Domain.Identifiers
open Organizations.Domain.FormaPrawna
open Organizations.Domain.Organization

type OrganizationState = {
    Teczka: TeczkaId
    IdentyfikatorEnova: string
    NIP: Nip
    Regon: Regon
    FormaPrawna: FormaPrawna
    OPP: bool
    DaneAdresowe: DaneAdresowe
    Kontakty: Kontakty
    ZrodlaZywnosci: ZrodlaZywnosci
    AdresyKsiegowosci: AdresyKsiegowosci
    Beneficjenci: Beneficjenci
    WarunkiPomocy: WarunkiPomocy
    Version: int
}

module OrganizationState =
    let empty teczkaId = {
        Teczka = teczkaId
        IdentyfikatorEnova = ""
        NIP = match Nip.create "0000000000" with | Ok nip -> nip | Error _ -> failwith "Invalid default NIP"
        Regon = match Regon.create "000000000" with | Ok regon -> regon | Error _ -> failwith "Invalid default Regon"
        FormaPrawna = { Nazwa = ""; Rejestracja = PozaRejestrem "" }
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
            WwwFacebook = ""
            Telefon = ""
            Przedstawiciel = ""
            Kontakt = ""
            Email = ""
            Dostepnosc = ""
            OsobaDoKontaktu = ""
            TelefonOsobyKontaktowej = ""
            MailOsobyKontaktowej = ""
            OsobaOdbierajacaZywnosc = ""
            TelefonOsobyOdbierajacej = ""
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
        Version = 0
    }

    let exists state = state.Version > 0
