module Organizations.Database.OrganizationsDao

open System.Data
open Organizations.Application.ReadModels
open Organizations.Domain.Identifiers
open Organizations.Domain.Organization
open PostgresPersistence.DapperFsharp
open Organizations.Application.Commands

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
    Wniosek: System.DateOnly option 
    UmowaZDn: System.DateOnly option  
    UmowaRODO: System.DateOnly option 
    KartyOrganizacjiData: System.DateOnly option 
    OstatnieOdwiedzinyData: System.DateOnly option 
    DataUpowaznieniaDoOdbioru: System.DateOnly option 
} with 
    static member From (org: Organization) =
        {
            Teczka = org.Teczka |> TeczkaId.unwrap
            IdentyfikatorEnova = org.IdentyfikatorEnova
            NIP = org.NIP |> Nip.unwrap
            Regon = org.Regon |> Regon.unwrap
            KrsNr = org.KrsNr |> Krs.unwrap
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
            Wniosek = org.Dokumenty.Wniosek
            UmowaZDn = org.Dokumenty.UmowaZDn
            UmowaRODO = org.Dokumenty.UmowaRODO
            KartyOrganizacjiData = org.Dokumenty.KartyOrganizacjiData
            OstatnieOdwiedzinyData = org.Dokumenty.OstatnieOdwiedzinyData
            DataUpowaznieniaDoOdbioru = org.Dokumenty.DataUpowaznieniaDoOdbioru
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
            Dokumenty = {
                Wniosek = this.Wniosek
                UmowaZDn = this.UmowaZDn
                UmowaRODO = this.UmowaRODO
                KartyOrganizacjiData = this.KartyOrganizacjiData
                OstatnieOdwiedzinyData = this.OstatnieOdwiedzinyData
                DataUpowaznieniaDoOdbioru = this.DataUpowaznieniaDoOdbioru
            }
        }


let searchOrgsSql sortBy dir = $"""
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
    kategoria,
    ostatnieodwiedzinydata
FROM organizacje
WHERE
   @searchTerm = '' 
   OR teczka = CASE WHEN @searchTerm ~ '^\d+$' THEN @searchTerm::bigint  END
   OR (similarity(nazwaplacowkitrafiazywnosc, @searchTerm) > 0.2 OR similarity(gminadzielnica, @searchTerm) > 0.2)
ORDER BY %s{sortBy} %s{dir};
"""

let readSummaries (connectDB: unit -> Async<IDbConnection>) (filter: Filter) =
    async {
        use! db = connectDB()
        let sortBy, dir = match filter.sortBy with
                            | Some (sortBy, dir) -> (sortBy, dir.ToString())
                            | None -> ("teczka", "desc")
        let query = (searchOrgsSql sortBy dir)
        return! db.QueryBy<OrganizationSummary> query {| searchTerm = filter.searchTerm |}
    }
    
let changeDaneAdresowe (connectDB: unit -> Async<IDbConnection>) (teczkaId: TeczkaId, daneAdresowe: DaneAdresowe) =
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
WHERE Teczka = @Teczka;""" {|
                              Teczka = teczkaId
                              NazwaOrganizacjiPodpisujacejUmowe = daneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
                              AdresRejestrowy = daneAdresowe.AdresRejestrowy
                              NazwaPlacowkiTrafiaZywnosc = daneAdresowe.NazwaPlacowkiTrafiaZywnosc
                              AdresPlacowkiTrafiaZywnosc = daneAdresowe.AdresPlacowkiTrafiaZywnosc
                              GminaDzielnica = daneAdresowe.GminaDzielnica
                              Powiat = daneAdresowe.Powiat
                            |}
    }
    
let changeKontakty (connectDB: unit -> Async<IDbConnection>) (teczkaId: TeczkaId, kontakty: Kontakty) =
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
WHERE Teczka = @Teczka;""" {|
                                Teczka = teczkaId
                                WwwFacebook = kontakty.WwwFacebook
                                Telefon = kontakty.Telefon
                                Przedstawiciel = kontakty.Przedstawiciel
                                Kontakt = kontakty.Kontakt
                                Email = kontakty.Email
                                Dostepnosc = kontakty.Dostepnosc
                                OsobaDoKontaktu = kontakty.OsobaDoKontaktu
                                TelefonOsobyKontaktowej = kontakty.TelefonOsobyKontaktowej
                                MailOsobyKontaktowej = kontakty.MailOsobyKontaktowej
                                OsobaOdbierajacaZywnosc = kontakty.OsobaOdbierajacaZywnosc
                                TelefonOsobyOdbierajacej = kontakty.TelefonOsobyOdbierajacej
                            |}
    }
    
let changeBeneficjenci (connectDB: unit -> Async<IDbConnection>) (teczkaId: TeczkaId, beneficjenci: Beneficjenci) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    LiczbaBeneficjentow = @LiczbaBeneficjentow,
    Beneficjenci = @Beneficjenci
WHERE Teczka = @Teczka;""" {|
                             Teczka = teczkaId
                             LiczbaBeneficjentow = beneficjenci.LiczbaBeneficjentow
                             Beneficjenci = beneficjenci.Beneficjenci
                             |}
    }
    
let changeDokumenty (connectDB: unit -> Async<IDbConnection>) (teczkaId: TeczkaId, dokumenty: Dokumenty) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    Wniosek = @Wniosek,
    UmowaZDn = @UmowaZDn,
    UmowaRODO = @UmowaRODO,
    KartyOrganizacjiData = @KartyOrganizacjiData,
    OstatnieOdwiedzinyData = @OstatnieOdwiedzinyData,
    DataUpowaznieniaDoOdbioru = @DataUpowaznieniaDoOdbioru
WHERE Teczka = @Teczka;""" {|
                Teczka = teczkaId
                Wniosek = dokumenty.Wniosek
                UmowaZDn = dokumenty.UmowaZDn
                UmowaRODO = dokumenty.UmowaRODO
                KartyOrganizacjiData = dokumenty.KartyOrganizacjiData
                OstatnieOdwiedzinyData = dokumenty.OstatnieOdwiedzinyData
                DataUpowaznieniaDoOdbioru = dokumenty.DataUpowaznieniaDoOdbioru
            |}
    }
    
let changeZrodlaZywnosci (connectDB: unit -> Async<IDbConnection>) (teczkaId: TeczkaId, zrodlaZywnosci: ZrodlaZywnosci) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    bazarki = @Bazarki,
    machfit = @Machfit,
    sieci = @Sieci,
    fepz2024 = @FEPZ2024,
    odbiorkrotkitermin = @OdbiorKrotkiTermin,
    tylkonaszmagazyn = @TylkoNaszMagazyn
WHERE Teczka = @Teczka;""" {|
                Teczka = teczkaId
                Bazarki = zrodlaZywnosci.Bazarki
                Machfit = zrodlaZywnosci.Machfit
                Sieci = zrodlaZywnosci.Sieci
                FEPZ2024 = zrodlaZywnosci.FEPZ2024
                OdbiorKrotkiTermin = zrodlaZywnosci.OdbiorKrotkiTermin
                TylkoNaszMagazyn = zrodlaZywnosci.TylkoNaszMagazyn
            |}
    }
    
let changeWarunkiPomocy (connectDB: unit -> Async<IDbConnection>) (teczkaId: TeczkaId, warunkiPomocy: WarunkiPomocy) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    kategoria = @Kategoria,
    sanepid = @Sanepid,
    rodzajPomocy = @RodzajPomocy,
    transportKategoria = @TransportKategoria,
    TransportOpis = @TransportOpis,
    WarunkiMagazynowe = @WarunkiMagazynowe,
    SposobUdzielaniaPomocy = @SposobUdzielaniaPomocy,
    HACCP = @HACCP
WHERE Teczka = @Teczka;""" {|
                Teczka = teczkaId
                Kategoria = warunkiPomocy.Kategoria
                Sanepid = warunkiPomocy.Sanepid
                RodzajPomocy = warunkiPomocy.RodzajPomocy
                TransportKategoria = warunkiPomocy.TransportKategoria
                TransportOpis = warunkiPomocy.TransportOpis
                WarunkiMagazynowe = warunkiPomocy.WarunkiMagazynowe
                SposobUdzielaniaPomocy = warunkiPomocy.SposobUdzielaniaPomocy
                HACCP = warunkiPomocy.HACCP
            |}
    }
    
let changeAdresyKsiegowosci (connectDB: unit -> Async<IDbConnection>) (teczkaId: TeczkaId, adresy: AdresyKsiegowosci) =
    async {
        use! db = connectDB()
        do! db.Execute """
UPDATE organizacje
SET 
    KsiegowanieAdres = @KsiegowanieAdres,
    NazwaOrganizacjiKsiegowanieDarowizn = @NazwaOrganizacjiKsiegowanieDarowizn,
    TelOrganProwadzacegoKsiegowosc = @TelOrganProwadzacegoKsiegowosc
WHERE Teczka = @Teczka;""" {|
                Teczka = teczkaId
                KsiegowanieAdres = adresy.KsiegowanieAdres
                NazwaOrganizacjiKsiegowanieDarowizn = adresy.NazwaOrganizacjiKsiegowanieDarowizn
                TelOrganProwadzacegoKsiegowosc = adresy.TelOrganProwadzacegoKsiegowosc
            |}
    }
    
let readBy (connectDB: unit -> Async<IDbConnection>) (teczka: int64) =
    async {
        use! db = connectDB()
        let! row = db.Single<OrganizationDetailsRow> "SELECT * FROM organizacje WHERE teczka = @teczka" {|teczka = teczka|}
        return row.ToReadModel()
    }
    
let private saveOnConnection (dbConnection: IDbConnection) (org: Organization) =
    async {
        let row = OrganizationDetailsRow.From org
        do! row |> dbConnection.Execute """
    INSERT INTO organizacje (
        Teczka,
        IdentyfikatorEnova,
        NIP,
        Regon,
        KrsNr,
        FormaPrawna,
        OPP,
        NazwaOrganizacjiPodpisujacejUmowe,
        AdresRejestrowy,
        NazwaPlacowkiTrafiaZywnosc,
        AdresPlacowkiTrafiaZywnosc,
        GminaDzielnica,
        Powiat,
        NazwaOrganizacjiKsiegowanieDarowizn,
        KsiegowanieAdres,
        TelOrganProwadzacegoKsiegowosc,
        WwwFacebook,
        Telefon,
        Przedstawiciel,
        Kontakt,
        Email,
        Dostepnosc,
        OsobaDoKontaktu,
        TelefonOsobyKontaktowej,
        MailOsobyKontaktowej,
        OsobaOdbierajacaZywnosc,
        TelefonOsobyOdbierajacej,
        LiczbaBeneficjentow,
        Beneficjenci,
        Sieci,
        Bazarki,
        Machfit,
        FEPZ2024,
        OdbiorKrotkiTermin,
        TylkoNaszMagazyn,
        Kategoria,
        RodzajPomocy,
        SposobUdzielaniaPomocy,
        WarunkiMagazynowe,
        HACCP,
        Sanepid,
        TransportOpis,
        TransportKategoria,
        Wniosek,
        UmowaZDn,
        UmowaRODO,
        KartyOrganizacjiData,
        OstatnieOdwiedzinyData,
        DataUpowaznieniaDoOdbioru
    )
    VALUES (
        @Teczka,
        @IdentyfikatorEnova,
        @NIP,
        @Regon,
        @KrsNr,
        @FormaPrawna,
        @OPP,
        @NazwaOrganizacjiPodpisujacejUmowe,
        @AdresRejestrowy,
        @NazwaPlacowkiTrafiaZywnosc,
        @AdresPlacowkiTrafiaZywnosc,
        @GminaDzielnica,
        @Powiat,
        @NazwaOrganizacjiKsiegowanieDarowizn,
        @KsiegowanieAdres,
        @TelOrganProwadzacegoKsiegowosc,
        @WwwFacebook,
        @Telefon,
        @Przedstawiciel,
        @Kontakt,
        @Email,
        @Dostepnosc,
        @OsobaDoKontaktu,
        @TelefonOsobyKontaktowej,
        @MailOsobyKontaktowej,
        @OsobaOdbierajacaZywnosc,
        @TelefonOsobyOdbierajacej,
        @LiczbaBeneficjentow,
        @Beneficjenci,
        @Sieci,
        @Bazarki,
        @Machfit,
        @FEPZ2024,
        @OdbiorKrotkiTermin,
        @TylkoNaszMagazyn,
        @Kategoria,
        @RodzajPomocy,
        @SposobUdzielaniaPomocy,
        @WarunkiMagazynowe,
        @HACCP,
        @Sanepid,
        @TransportOpis,
        @TransportKategoria,
        @Wniosek,
        @UmowaZDn,
        @UmowaRODO,
        @KartyOrganizacjiData,
        @OstatnieOdwiedzinyData,
        @DataUpowaznieniaDoOdbioru
    )
ON CONFLICT DO NOTHING;
    """
    }
    
let saveMany (connectDB: unit -> Async<IDbConnection>) (orgs: Organization list)=
    async {
        use! db = connectDB()
        let tasks = orgs|> List.map(saveOnConnection db)
        for task in tasks do
            do! task
    }
    
let save (connectDB: unit -> Async<IDbConnection>) (org: Organization)  =
    async {
        use! db = connectDB()
        do! org |> saveOnConnection db 
    }
