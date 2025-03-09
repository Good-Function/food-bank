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
            NazwaOrganizacjiKsiegowanieDarowizn = org.NazwaOrganizacjiKsiegowanieDarowizn
            KsiegowanieAdres = org.KsiegowanieAdres
            TelOrganProwadzacegoKsiegowosc = org.TelOrganProwadzacegoKsiegowosc
            WwwFacebook = org.WwwFacebook
            Telefon = org.Telefon
            Przedstawiciel = org.Przedstawiciel
            Kontakt = org.Kontakt
            Email = org.Email
            Dostepnosc = org.Dostepnosc
            OsobaDoKontaktu = org.OsobaDoKontaktu
            TelefonOsobyKontaktowej = org.TelefonOsobyKontaktowej
            MailOsobyKontaktowej = org.MailOsobyKontaktowej
            OsobaOdbierajacaZywnosc = org.OsobaOdbierajacaZywnosc
            TelefonOsobyOdbierajacej = org.TelefonOsobyOdbierajacej
            LiczbaBeneficjentow = org.LiczbaBeneficjentow
            Beneficjenci = org.Beneficjenci
            Sieci = org.Sieci
            Bazarki = org.Bazarki
            Machfit = org.Machfit
            FEPZ2024 = org.FEPZ2024
            Kategoria = org.Kategoria
            RodzajPomocy = org.RodzajPomocy
            SposobUdzielaniaPomocy = org.SposobUdzielaniaPomocy
            WarunkiMagazynowe = org.WarunkiMagazynowe
            HACCP = org.HACCP
            Sanepid = org.Sanepid
            TransportOpis = org.TransportOpis
            TransportKategoria = org.TransportKategoria
            Wniosek = org.Wniosek
            UmowaZDn = org.UmowaZDn
            UmowaRODO = org.UmowaRODO
            KartyOrganizacjiData = org.KartyOrganizacjiData
            OstatnieOdwiedzinyData = org.OstatnieOdwiedzinyData
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
            NazwaOrganizacjiKsiegowanieDarowizn = this.NazwaOrganizacjiKsiegowanieDarowizn
            KsiegowanieAdres = this.KsiegowanieAdres
            TelOrganProwadzacegoKsiegowosc = this.TelOrganProwadzacegoKsiegowosc
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
            LiczbaBeneficjentow = this.LiczbaBeneficjentow
            Beneficjenci = this.Beneficjenci
            Sieci = this.Sieci
            Bazarki = this.Bazarki
            Machfit = this.Machfit
            FEPZ2024 = this.FEPZ2024
            Kategoria = this.Kategoria
            RodzajPomocy = this.RodzajPomocy
            SposobUdzielaniaPomocy = this.SposobUdzielaniaPomocy
            WarunkiMagazynowe = this.WarunkiMagazynowe
            HACCP = this.HACCP
            Sanepid = this.Sanepid
            TransportOpis = this.TransportOpis
            TransportKategoria = this.TransportKategoria
            Wniosek = this.Wniosek
            UmowaZDn = this.UmowaZDn
            UmowaRODO = this.UmowaRODO
            KartyOrganizacjiData = this.KartyOrganizacjiData
            OstatnieOdwiedzinyData = this.OstatnieOdwiedzinyData
        }


let prepareSearchSql orderBy = $"""
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
    OstatnieOdwiedzinyData 
FROM organizacje
WHERE
   @searchTerm = '' 
   OR (@searchTerm ~ '^\d+$' AND teczka = CAST(@searchTerm AS BIGINT))
   OR (similarity(nazwaplacowkitrafiazywnosc, @searchTerm) > 0.1 OR similarity(gminadzielnica, @searchTerm) > 0.1)
ORDER BY %s{orderBy} ASC;
"""

let readSummaries (connectDB: unit -> Async<IDbConnection>) (searchTerm: string, orderBy: string) =
    async {
        let allowedOrderColumns = ["teczka"; "ostatnieodwiedzinydata"; "nazwaplacowkitrafiazywnosc"; "gminadzielnica"]
        let orderByWhitelisted = allowedOrderColumns 
                                    |> List.tryFind ((=) (orderBy.ToLower()))
                                    |> Option.defaultValue "teczka"
        let filledSearchQuery = prepareSearchSql orderByWhitelisted
        let! db = connectDB()
        return! db.QueryBy<OrganizationSummary> filledSearchQuery {| searchTerm = searchTerm; orderBy = orderBy|}
    }
    
let modifyDaneAdresowe (connectDB: unit -> Async<IDbConnection>) (daneAdresowe: Commands.DaneAdresowe) =
    async {
        let! db = connectDB()
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
    
let readBy (connectDB: unit -> Async<IDbConnection>) (teczka: int64) =
    async {
        let! db = connectDB()
        let! row = db.Single<OrganizationDetailsRow> "SELECT * FROM organizacje WHERE teczka = @teczka" {|teczka = teczka|}
        return row.ToReadModel()
    }
    
let save (connectDB: unit -> Async<IDbConnection>) (org: OrganizationDetails)  =
    async {
        let! db = connectDB()
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
