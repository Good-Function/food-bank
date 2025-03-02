module Organizations.Database.OrganizationsDao

open System.Data
open PostgresPersistence.DapperFsharp
open Organizations.Application.ReadModels

let readSummaries (connectDB: unit -> Async<IDbConnection>) (searchTerm: string) =
    async {
        let! db = connectDB()
        let! summaries = db.Query<OrganizationSummary> """
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
FROM organizacje ORDER BY teczka 
"""
        let data = summaries
                |> List.filter (fun sum ->
                        sum.Teczka.ToString().Contains searchTerm || sum.NazwaPlacowkiTrafiaZywnosc.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
        return data
    }

let searchSummaries (connectDB: unit -> Async<IDbConnection>) (searchTerm: string) =
    async {
        let! db = connectDB()
        let parameters = Dapper.DynamicParameters()
        parameters.Add("@searchTerm", searchTerm)

        let! summaries = db.QueryWithParam<OrganizationSummary>("""
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
WHERE similarity(nazwaplacowkitrafiazywnosc, @searchTerm) > 0.1
   OR similarity(gminadzielnica, @searchTerm) > 0.1
ORDER BY teczka;
""", parameters)
        return summaries
    }

let readBy (connectDB: unit -> Async<IDbConnection>) (teczka: int64) =
    async {
        let! db = connectDB()
        return! db.Single<OrganizationDetails> """
SELECT * FROM organizacje WHERE teczka = @teczka
""" {|teczka = teczka|}
    }
    
let save (connectDB: unit -> Async<IDbConnection>) (org: OrganizationDetails)  =
    async {
        let! db = connectDB()
        do! org |> db.Execute """
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
