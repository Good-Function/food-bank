module Organizations.Database.OrganizationsDao

open System.Data
open Organizations.Application
open Organizations.Application.DocumentType
open Organizations.Application.ReadModels
open Organizations.Application.ReadModels.MailingList
open Organizations.Application.ReadModels.OrganizationSummary
open Organizations.Database.DateOnlyCoder
open Organizations.Domain.Organization
open PostgresPersistence.DapperFsharp
open Thoth.Json.Net
open OrganizationRow

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
        
let readDetailsBy (connectDB: unit -> Async<IDbConnection>) (teczka: int64) =
    async {
        use! db = connectDB()
        let! row = db.Single<OrganizationDetailsRow> "SELECT * FROM organizacje WHERE teczka = @teczka" {|teczka = teczka|}
        return row.ToReadModel()
    }
    
let readByEmail (connectDB: unit -> Async<IDbConnection>): OrganizationDetails.ReadOrganizationDetailsByEmail =
    fun email ->
        async {
            use! db = connectDB()
            let! row = db.Single<OrganizationDetailsRow> "SELECT * FROM organizacje WHERE email = @email" {|email = email|}
            return row.ToReadModel()
        }

let readBy (connectDB: unit -> Async<IDbConnection>) (teczka: Organizations.Domain.Identifiers.TeczkaId) =
    async {
        use! db = connectDB()
        let! row = db.Single<OrganizationDetailsRow> "SELECT * FROM organizacje WHERE teczka = @teczka" {|teczka = teczka |> Organizations.Domain.Identifiers.TeczkaId.unwrap|}
        return row.ToDomain()
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
ON CONFLICT (Teczka) DO UPDATE
    SET
        IdentyfikatorEnova = @IdentyfikatorEnova,
        NIP = @NIP,
        Regon = @Regon,
        KrsNr = @KrsNr,
        FormaPrawna = @FormaPrawna,
        OPP = @OPP,
        NazwaOrganizacjiPodpisujacejUmowe = @NazwaOrganizacjiPodpisujacejUmowe,
        AdresRejestrowy = @AdresRejestrowy,
        NazwaPlacowkiTrafiaZywnosc = @NazwaPlacowkiTrafiaZywnosc,
        AdresPlacowkiTrafiaZywnosc = @AdresPlacowkiTrafiaZywnosc,
        GminaDzielnica = @GminaDzielnica,
        Powiat = @Powiat,
        NazwaOrganizacjiKsiegowanieDarowizn = @NazwaOrganizacjiKsiegowanieDarowizn,
        KsiegowanieAdres = @KsiegowanieAdres,
        TelOrganProwadzacegoKsiegowosc = @TelOrganProwadzacegoKsiegowosc,
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
        TelefonOsobyOdbierajacej = @TelefonOsobyOdbierajacej,
        LiczbaBeneficjentow = @LiczbaBeneficjentow,
        Beneficjenci = @Beneficjenci,
        Sieci = @Sieci,
        Bazarki = @Bazarki,
        Machfit = @Machfit,
        FEPZ2024 = @FEPZ2024,
        OdbiorKrotkiTermin = @OdbiorKrotkiTermin,
        TylkoNaszMagazyn = @TylkoNaszMagazyn,
        Kategoria = @Kategoria,
        RodzajPomocy = @RodzajPomocy,
        SposobUdzielaniaPomocy = @SposobUdzielaniaPomocy,
        WarunkiMagazynowe = @WarunkiMagazynowe,
        HACCP = @HACCP,
        Sanepid = @Sanepid,
        TransportOpis = @TransportOpis,
        TransportKategoria = @TransportKategoria,
        Documents = @Documents::jsonb;
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
