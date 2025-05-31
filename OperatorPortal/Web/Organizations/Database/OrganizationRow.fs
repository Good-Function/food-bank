module Web.Organizations.Database.OrganizationRow

open System
open System.Text.Json
open Organizations.Application
open Organizations.Application.ReadModels
open Organizations.Domain.FormaPrawna
open Organizations.Domain.Identifiers
open Organizations.Domain.Organization

type Document = {
    Date: DateOnly option
    FileName: string option
}

type OrganizationDetailsRow = {
    Teczka: int64
    IdentyfikatorEnova: string
    NIP: string 
    Regon: string 
    KrsNr: string 
    FormaPrawna: string 
    OPP: bool 
    NazwaOrganizacjiPodpisujacejUmowe: string 
    AdresRejestrowy: string 
    NazwaPlacowkiTrafiaZywnosc: string 
    AdresPlacowkiTrafiaZywnosc: string 
    GminaDzielnica: string 
    Powiat: string 
    NazwaOrganizacjiKsiegowanieDarowizn: string
    KsiegowanieAdres: string 
    TelOrganProwadzacegoKsiegowosc: string 
    WwwFacebook: string 
    Telefon: string 
    Przedstawiciel: string 
    Kontakt: string 
    Email: string 
    Dostepnosc: string 
    OsobaDoKontaktu: string 
    TelefonOsobyKontaktowej: string 
    MailOsobyKontaktowej: string 
    OsobaOdbierajacaZywnosc: string
    TelefonOsobyOdbierajacej: string 
    LiczbaBeneficjentow: int 
    Beneficjenci: string 
    Sieci: bool 
    Bazarki: bool
    Machfit: bool
    FEPZ2024: bool
    OdbiorKrotkiTermin: bool
    TylkoNaszMagazyn: bool
    Kategoria: string 
    RodzajPomocy: string 
    SposobUdzielaniaPomocy: string 
    WarunkiMagazynowe: string 
    HACCP: bool 
    Sanepid: bool 
    TransportOpis: string 
    TransportKategoria: string 
    Documents: string
} with 
    static member From (org: Organization) =
        {
            Teczka = org.Teczka |> TeczkaId.unwrap
            IdentyfikatorEnova = org.IdentyfikatorEnova
            NIP = org.NIP |> Nip.unwrap
            Regon = org.Regon |> Regon.unwrap
            KrsNr = org.FormaPrawna.Rejestracja |> function
                | PozaRejestrem nr -> nr
                | WRejestrzeKRS krs -> Krs.unwrap krs
            FormaPrawna = org.FormaPrawna.Nazwa
            OPP = org.OPP
            NazwaOrganizacjiPodpisujacejUmowe = org.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
            AdresRejestrowy = org.DaneAdresowe.AdresRejestrowy
            NazwaPlacowkiTrafiaZywnosc = org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            AdresPlacowkiTrafiaZywnosc = org.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
            GminaDzielnica = org.DaneAdresowe.GminaDzielnica
            Powiat = org.DaneAdresowe.Powiat
            NazwaOrganizacjiKsiegowanieDarowizn = org.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
            KsiegowanieAdres = org.AdresyKsiegowosci.KsiegowanieAdres
            TelOrganProwadzacegoKsiegowosc = org.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
            WwwFacebook = org.Kontakty.WwwFacebook
            Telefon = org.Kontakty.Telefon
            Przedstawiciel = org.Kontakty.Przedstawiciel
            Kontakt = org.Kontakty.Kontakt
            Email = org.Kontakty.Email
            Dostepnosc = org.Kontakty.Dostepnosc
            OsobaDoKontaktu = org.Kontakty.OsobaDoKontaktu
            TelefonOsobyKontaktowej = org.Kontakty.TelefonOsobyKontaktowej
            MailOsobyKontaktowej = org.Kontakty.MailOsobyKontaktowej
            OsobaOdbierajacaZywnosc = org.Kontakty.OsobaOdbierajacaZywnosc
            TelefonOsobyOdbierajacej = org.Kontakty.TelefonOsobyOdbierajacej
            LiczbaBeneficjentow = org.Beneficjenci.LiczbaBeneficjentow
            Beneficjenci = org.Beneficjenci.Beneficjenci
            Sieci = org.ZrodlaZywnosci.Sieci
            Bazarki = org.ZrodlaZywnosci.Bazarki
            Machfit = org.ZrodlaZywnosci.Machfit
            FEPZ2024 = org.ZrodlaZywnosci.FEPZ2024
            OdbiorKrotkiTermin = org.ZrodlaZywnosci.OdbiorKrotkiTermin
            TylkoNaszMagazyn = org.ZrodlaZywnosci.TylkoNaszMagazyn
            Kategoria = org.WarunkiPomocy.Kategoria
            RodzajPomocy = org.WarunkiPomocy.RodzajPomocy
            SposobUdzielaniaPomocy = org.WarunkiPomocy.SposobUdzielaniaPomocy
            WarunkiMagazynowe = org.WarunkiPomocy.WarunkiMagazynowe
            HACCP = org.WarunkiPomocy.HACCP
            Sanepid = org.WarunkiPomocy.Sanepid
            TransportOpis = org.WarunkiPomocy.TransportOpis
            TransportKategoria = org.WarunkiPomocy.TransportKategoria
            Documents = JsonSerializer.Serialize(org.Dokumenty)
        }
    member this.ToReadModel (): OrganizationDetails =
        {
            Teczka = this.Teczka
            IdentyfikatorEnova = this.IdentyfikatorEnova
            NIP = this.NIP
            Regon = this.Regon
            KrsNr = this.KrsNr
            FormaPrawna = this.FormaPrawna
            OPP = this.OPP
            DaneAdresowe = {
                NazwaOrganizacjiPodpisujacejUmowe = this.NazwaOrganizacjiPodpisujacejUmowe
                AdresRejestrowy = this.AdresRejestrowy
                NazwaPlacowkiTrafiaZywnosc = this.NazwaPlacowkiTrafiaZywnosc
                AdresPlacowkiTrafiaZywnosc = this.AdresPlacowkiTrafiaZywnosc
                GminaDzielnica = this.GminaDzielnica
                Powiat = this.Powiat
            }
            AdresyKsiegowosci = {
                NazwaOrganizacjiKsiegowanieDarowizn = this.NazwaOrganizacjiKsiegowanieDarowizn
                KsiegowanieAdres = this.KsiegowanieAdres
                TelOrganProwadzacegoKsiegowosc = this.TelOrganProwadzacegoKsiegowosc      
            }
            Kontakty = {
                WwwFacebook = this.WwwFacebook
                Telefon = this.Telefon
                Przedstawiciel = this.Przedstawiciel
                Kontakt = this.Kontakt
                Email = this.Email
                Dostepnosc = this.Dostepnosc
                OsobaDoKontaktu = this.OsobaDoKontaktu
                TelefonOsobyKontaktowej = this.TelefonOsobyKontaktowej
                MailOsobyKontaktowej = this.MailOsobyKontaktowej
                OsobaOdbierajacaZywnosc = this.OsobaOdbierajacaZywnosc
                TelefonOsobyOdbierajacej = this.TelefonOsobyOdbierajacej
            }
            Beneficjenci = {
                Beneficjenci = this.Beneficjenci
                LiczbaBeneficjentow = this.LiczbaBeneficjentow
            }
            ZrodlaZywnosci = {
                Sieci = this.Sieci
                Bazarki = this.Bazarki
                Machfit = this.Machfit
                FEPZ2024 = this.FEPZ2024
                OdbiorKrotkiTermin = this.OdbiorKrotkiTermin
                TylkoNaszMagazyn = this.TylkoNaszMagazyn
            }
            WarunkiPomocy = {
                Kategoria = this.Kategoria
                RodzajPomocy = this.RodzajPomocy
                SposobUdzielaniaPomocy = this.SposobUdzielaniaPomocy
                WarunkiMagazynowe = this.WarunkiMagazynowe
                HACCP = this.HACCP
                Sanepid = this.Sanepid
                TransportOpis = this.TransportOpis
                TransportKategoria = this.TransportKategoria
            }
            Dokumenty = JsonSerializer.Deserialize<Documents>(this.Documents) |> fun docs -> [{
                Date = docs.Wniosek.Date
                FileName = docs.Wniosek.FileName
                Type = DocumentType.Wniosek
            };
            {
                Date = docs.Umowa.Date
                FileName = docs.Umowa.FileName
                Type = DocumentType.Umowa
            };
            {
                Date = docs.Odwiedziny.Date
                FileName = docs.Odwiedziny.FileName
                Type = DocumentType.Odwiedziny
            };
            {
                Date = docs.Rodo.Date
                FileName = docs.Rodo.FileName
                Type = DocumentType.RODO
            };
            {
                Date = docs.UpowaznienieDoOdbioru.Date
                FileName = docs.UpowaznienieDoOdbioru.FileName
                Type = DocumentType.UpowaznienieDoOdbioru
            }]
        }