module Organizations.Database.OrganizationsDao

open System.Data
open Organizations.Application
open Organizations.Application.ReadModels
open Organizations.Domain.Organization
open PostgresPersistence.DapperFsharp
open Organizations.Application.Commands
open Web.Organizations.Database.OrganizationRow

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
    jsonb_extract_path_text(documents, 'Odwiedziny', 'Date')::date AS ostatnieodwiedzinydata
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
    OstatnieOdwiedzinyData = @OstatnieOdwiedzinyData,
    DataUpowaznieniaDoOdbioru = @DataUpowaznieniaDoOdbioru
WHERE Teczka = @Teczka;""" {|
                Teczka = teczkaId
                Wniosek = dokumenty.WniosekDate
                UmowaZDn = dokumenty.UmowaDate
                UmowaRODO = dokumenty.RODODate
                OstatnieOdwiedzinyData = dokumenty.OdwiedzinyDate
                DataUpowaznieniaDoOdbioru = dokumenty.UpowaznienieDoOdbioruDate
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
        Documents
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
        @Documents::jsonb
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
