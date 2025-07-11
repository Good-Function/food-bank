module Organizations.Database.OrganizationsDao

open System
open System.Data
open Organizations.Application
open Organizations.Application.DocumentType
open Organizations.Application.ReadModels
open Organizations.Application.ReadModels.MailingList
open Organizations.Application.ReadModels.OrganizationSummary
open Organizations.Database.DateOnlyCoder
open Organizations.Domain.Organization
open PostgresPersistence.DapperFsharp
open Organizations.Application.Commands
open Thoth.Json.Net
open Web.Organizations.Database.OrganizationRow

let toDb (opt: 'a option) =
    match opt with
    | Some x -> x
    | None -> null

let searchOrgsSql sortBy dir filterClause = $"""
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
    beneficjenci,
    liczbabeneficjentow,
    kategoria,
    jsonb_extract_path_text(documents, 'Odwiedziny', 'Date')::date AS ostatnieodwiedzinydata
FROM organizacje
WHERE
   (@searchTerm = '' 
   OR teczka = CASE WHEN @searchTerm ~ '^\d+$' THEN @searchTerm::bigint  END
   OR (similarity(nazwaplacowkitrafiazywnosc, @searchTerm) > 0.2 OR similarity(gminadzielnica, @searchTerm) > 0.2))
   {filterClause}
ORDER BY %s{sortBy} %s{dir}
OFFSET @offset
LIMIT @size;
"""

let searchMailsSql filterClause = $"""
SELECT teczka, email
FROM organizacje
WHERE
   (@searchTerm = '' 
   OR teczka = CASE WHEN @searchTerm ~ '^\d+$' THEN @searchTerm::bigint  END
   OR (similarity(nazwaplacowkitrafiazywnosc, @searchTerm) > 0.2 OR similarity(gminadzielnica, @searchTerm) > 0.2))
   {filterClause}
"""

let searchOrgsCountSql filterClause = $"""
SELECT COUNT(*)
FROM organizacje
WHERE
   (@searchTerm = '' 
   OR teczka = CASE WHEN @searchTerm ~ '^\d+$' THEN @searchTerm::bigint  END
   OR (similarity(nazwaplacowkitrafiazywnosc, @searchTerm) > 0.2 OR similarity(gminadzielnica, @searchTerm) > 0.2))
   {filterClause}
"""
    
let prepareFilter (operator: string, value: obj) =
    match value with
    | :? int -> $"{operator} {value}"
    | :? string -> $"{operator} '%%{value}%%'"
    | _ -> failwith  "Unknown type"

let readSummaries (connectDB: unit -> Async<IDbConnection>): ReadOrganizationSummaries =
    fun query ->
        async {
            use! db = connectDB()
            let sortBy, dir = match query.SortBy with
                                | Some (sortBy, dir) -> ($"{sortBy}", dir.ToString())
                                | None -> ("teczka", "desc")
            let filterClause = query.Filters
                               |> List.map(fun f -> $"AND {f.Key} {(prepareFilter(f.Operator.Symbol, f.Value))} ")
                               |> String.concat ""
            let summariesSql = (searchOrgsSql sortBy dir filterClause)
            let totalSql = (searchOrgsCountSql filterClause)
            let! total = db.Single<int> totalSql {| searchTerm = query.SearchTerm |}
            let! summaries = db.QueryBy<OrganizationSummary> summariesSql
                                {|
                                   searchTerm = query.SearchTerm
                                   size = query.Pagination.Size
                                   offset = query.Pagination.Size * (query.Pagination.Page - 1)
                                |}
            return summaries, total
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

let saveDocMetadata (connectDB: unit -> Async<IDbConnection>) : DocumentHandlers.SaveDocMetadata =
    fun (teczkaId, metadata) ->
        async {
            use! db = connectDB()
            let jsonPropName =
                match metadata.Type with
                | Odwiedziny -> "Odwiedziny"
                | Umowa -> "Umowa"
                | RODO -> "Rodo"
                | UpowaznienieDoOdbioru -> "UpowaznienieDoOdbioru"
                | Wniosek -> "Wniosek"
            do! db.Execute $"""
UPDATE organizacje
SET Documents = jsonb_set(
    Documents,
    '{{{jsonPropName}}}',
    @Jsonb::jsonb,
    true
)
WHERE Teczka = @Teczka; """ {|
                                Teczka = teczkaId
                                Jsonb = Encode.Auto.toString(0, {|Date= metadata.Date; FileName = metadata.FileName |}, CaseStrategy.PascalCase, Extra.empty |> (Extra.withCustom DateOnlyCodec.encode DateOnlyCodec.decode) )
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
    
let readMailingListBy (connectDB: unit-> Async<IDbConnection>): MailingList.ReadMailingList =
    fun (searchTerm, filters) ->
        async {
            let! db = connectDB()
            let filterClause = filters
                               |> List.map(fun f -> $"AND {f.Key} {(prepareFilter(f.Operator.Symbol, f.Value))} ")
                               |> String.concat ""
            let querySql = searchMailsSql filterClause
            let! mails = db.QueryBy<Contact> querySql
                                {| searchTerm = searchTerm |}
            return mails
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
