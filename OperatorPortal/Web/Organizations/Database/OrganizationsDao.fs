module Organizations.Database.OrganizationsDao

open System.Data
open Organizations.Application
open Organizations.Application.ReadModels
open PostgresPersistence.DapperFsharp

type OrganizationDetailsRow = {
    Teczka: int64
    IdentyfikatorEnova: int64
    NIP: int64 
    Regon: int64 
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
    Kategoria: string 
    RodzajPomocy: string 
    SposobUdzielaniaPomocy: string 
    WarunkiMagazynowe: string 
    HACCP: bool 
    Sanepid: bool 
    TransportOpis: string 
    TransportKategoria: string 
    Wniosek: System.DateOnly option 
    UmowaZDn: System.DateOnly option  
    UmowaRODO: System.DateOnly option 
    KartyOrganizacjiData: System.DateOnly option 
    OstatnieOdwiedzinyData: System.DateOnly option 
} with 
    static member From (org: OrganizationDetails) =
        {
            Teczka = org.Teczka
            IdentyfikatorEnova = org.IdentyfikatorEnova
            NIP = org.NIP
            Regon = org.Regon
            KrsNr = org.KrsNr 
            FormaPrawna = org.FormaPrawna
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
            Kategoria = org.Kategoria
            RodzajPomocy = org.RodzajPomocy
            SposobUdzielaniaPomocy = org.SposobUdzielaniaPomocy
            WarunkiMagazynowe = org.WarunkiMagazynowe
            HACCP = org.HACCP
            Sanepid = org.Sanepid
            TransportOpis = org.TransportOpis
            TransportKategoria = org.TransportKategoria
            Wniosek = org.Dokumenty.Wniosek
            UmowaZDn = org.Dokumenty.UmowaZDn
            UmowaRODO = org.Dokumenty.UmowaRODO
            KartyOrganizacjiData = org.Dokumenty.KartyOrganizacjiData
            OstatnieOdwiedzinyData = org.Dokumenty.OstatnieOdwiedzinyData
        }
    member this.ToReadModel () =
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
            }
            Kategoria = this.Kategoria
            RodzajPomocy = this.RodzajPomocy
            SposobUdzielaniaPomocy = this.SposobUdzielaniaPomocy
            WarunkiMagazynowe = this.WarunkiMagazynowe
            HACCP = this.HACCP
            Sanepid = this.Sanepid
            TransportOpis = this.TransportOpis
            TransportKategoria = this.TransportKategoria
            Dokumenty = {
                Wniosek = this.Wniosek
                UmowaZDn = this.UmowaZDn
                UmowaRODO = this.UmowaRODO
                KartyOrganizacjiData = this.KartyOrganizacjiData
                OstatnieOdwiedzinyData = this.OstatnieOdwiedzinyData
            }
        }


let searchOrgsSql = """
SELECT 
    teczka,
    formaprawna,
    nazwaplacowkitrafiazywnosc,
    adresplacowkitrafiazywnosc,
    gminadzielnica,
    telefon,
    kontakt,
    email,
    dostepnosc,
    osobadokontaktu,
    telefonosobykontaktowej,
    liczbabeneficjentow,
    kategoria
FROM organizacje
WHERE
   @searchTerm = '' 
   OR (similarity(nazwaplacowkitrafiazywnosc, @searchTerm) > 0.1 OR similarity(gminadzielnica, @searchTerm) > 0.1)
ORDER BY teczka DESC;
"""

let readSummaries (connectDB: unit -> Async<IDbConnection>) (searchTerm: string) =
    async {
        use! db = connectDB()
        return! db.QueryBy<OrganizationSummary> searchOrgsSql {| searchTerm = searchTerm |}
    }
    
let changeDaneAdresowe (connectDB: unit -> Async<IDbConnection>) (daneAdresowe: Commands.DaneAdresowe) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    NazwaOrganizacjiPodpisujacejUmowe = @NazwaOrganizacjiPodpisujacejUmowe,
    AdresRejestrowy = @AdresRejestrowy,
    NazwaPlacowkiTrafiaZywnosc = @NazwaPlacowkiTrafiaZywnosc,
    AdresPlacowkiTrafiaZywnosc = @AdresPlacowkiTrafiaZywnosc,
    GminaDzielnica = @GminaDzielnica,
    Powiat = @Powiat
WHERE Teczka = @Teczka;""" daneAdresowe
    }
    
let changeKontakty (connectDB: unit -> Async<IDbConnection>) (kontakty: Commands.Kontakty) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    WwwFacebook = @WwwFacebook,
    Telefon = @Telefon,
    Przedstawiciel = @Przedstawiciel,
    Kontakt = @Kontakt,
    Email = @Email,
    Dostepnosc = @Dostepnosc,
    OsobaDoKontaktu = @OsobaDoKontaktu,
    TelefonOsobyKontaktowej = @TelefonOsobyKontaktowej,
    MailOsobyKontaktowej = @MailOsobyKontaktowej,
    OsobaOdbierajacaZywnosc = @OsobaOdbierajacaZywnosc,
    TelefonOsobyOdbierajacej = @TelefonOsobyOdbierajacej
WHERE Teczka = @Teczka;""" kontakty
    }
    
let changeBeneficjenci (connectDB: unit -> Async<IDbConnection>) (beneficjenci: Commands.Beneficjenci) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    LiczbaBeneficjentow = @LiczbaBeneficjentow,
    Beneficjenci = @Beneficjenci
WHERE Teczka = @Teczka;""" beneficjenci
    }
    
let changeDokumenty (connectDB: unit -> Async<IDbConnection>) (dokumenty: Commands.Dokumenty) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    Wniosek = @Wniosek,
    UmowaZDn = @UmowaZDn,
    UmowaRODO = @UmowaRODO,
    KartyOrganizacjiData = @KartyOrganizacjiData,
    OstatnieOdwiedzinyData = @OstatnieOdwiedzinyData
WHERE Teczka = @Teczka;""" dokumenty
    }
    
let changeZrodlaZywnosci (connectDB: unit -> Async<IDbConnection>) (zrodlaZywnosci: Commands.ZrodlaZywnosci) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    bazarki = @Bazarki,
    machfit = @Machfit,
    sieci = @Sieci,
    fepz2024 = @FEPZ2024
WHERE Teczka = @Teczka;""" zrodlaZywnosci
    }
    
let changeAdresyKsiegowosci (connectDB: unit -> Async<IDbConnection>) (adresy: Commands.AdresyKsiegowosci) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    KsiegowanieAdres = @KsiegowanieAdres,
    NazwaOrganizacjiKsiegowanieDarowizn = @NazwaOrganizacjiKsiegowanieDarowizn,
    TelOrganProwadzacegoKsiegowosc = @TelOrganProwadzacegoKsiegowosc
WHERE Teczka = @Teczka;""" adresy
    }
    
let readBy (connectDB: unit -> Async<IDbConnection>) (teczka: int64) =
    async {
        use! db = connectDB()
        let! row = db.Single<OrganizationDetailsRow> "SELECT * FROM organizacje WHERE teczka = @teczka" {|teczka = teczka|}
        return row.ToReadModel()
    }
    
let save (connectDB: unit -> Async<IDbConnection>) (org: OrganizationDetails)  =
    async {
        use! db = connectDB()
        let a = OrganizationDetailsRow.From org
        do! a |> db.Execute """
INSERT INTO organizacje (
    Teczka, IdentyfikatorEnova, NIP, Regon, KrsNr, FormaPrawna, OPP,
    NazwaOrganizacjiPodpisujacejUmowe, AdresRejestrowy, NazwaPlacowkiTrafiaZywnosc,
    AdresPlacowkiTrafiaZywnosc, GminaDzielnica, Powiat, NazwaOrganizacjiKsiegowanieDarowizn,
    KsiegowanieAdres, TelOrganProwadzacegoKsiegowosc, WwwFacebook, Telefon, Przedstawiciel,
    Kontakt, Email, Dostepnosc, OsobaDoKontaktu, TelefonOsobyKontaktowej,
    MailOsobyKontaktowej, OsobaOdbierajacaZywnosc, TelefonOsobyOdbierajacej,
    LiczbaBeneficjentow, Beneficjenci, Sieci, Bazarki, Machfit, FEPZ2024, Kategoria,
    RodzajPomocy, SposobUdzielaniaPomocy, WarunkiMagazynowe, HACCP, Sanepid,
    TransportOpis, TransportKategoria, Wniosek, UmowaZDn, UmowaRODO, KartyOrganizacjiData,
    OstatnieOdwiedzinyData
) 
VALUES (
    @Teczka, @IdentyfikatorEnova, @NIP, @Regon, @KrsNr, @FormaPrawna, @OPP,
    @NazwaOrganizacjiPodpisujacejUmowe, @AdresRejestrowy, @NazwaPlacowkiTrafiaZywnosc,
    @AdresPlacowkiTrafiaZywnosc, @GminaDzielnica, @Powiat, @NazwaOrganizacjiKsiegowanieDarowizn,
    @KsiegowanieAdres, @TelOrganProwadzacegoKsiegowosc, @WwwFacebook, @Telefon, @Przedstawiciel,
    @Kontakt, @Email, @Dostepnosc, @OsobaDoKontaktu, @TelefonOsobyKontaktowej,
    @MailOsobyKontaktowej, @OsobaOdbierajacaZywnosc, @TelefonOsobyOdbierajacej,
    @LiczbaBeneficjentow, @Beneficjenci, @Sieci, @Bazarki, @Machfit, @FEPZ2024, @Kategoria,
    @RodzajPomocy, @SposobUdzielaniaPomocy, @WarunkiMagazynowe, @HACCP, @Sanepid,
    @TransportOpis, @TransportKategoria, @Wniosek, @UmowaZDn, @UmowaRODO, @KartyOrganizacjiData,
    @OstatnieOdwiedzinyData
)
""" 
    }
