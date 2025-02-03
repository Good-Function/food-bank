module Tests.OrganizationsFromExcel.Tests

open PostgresPersistence
open Xunit
open FsUnit.Xunit
open Organizations.Database.csvLoader
open DapperFsharp
open Organizations.Database.OrganizationRow

let con = dbConnect("Host=localhost;Port=5432;User Id=postgres;Password=Strong!Passw0rd;Database=food_bank;")


[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__

let sampleOrgs = Orgs.GetSample()

let sqlSaveOrg = """
INSERT INTO organizacje (
    Teczka, IdentyfikatorEnova, NIP, Regon, KrsNr, FormaPrawna, OPP,
    NazwaOrganizacjiPodpisujacejUmowe, AdresRejestrowy, NazwaPlacowkiTrafiaZywnosc,
    AdresPlacowkiTrafiaZywnosc, GminaDzielnica, Powiat, NazwaOrganizacjiKsiegowanieDarowizn,
    KsiegowanieAdres, TelOrganProwadzacegoKsiegowosc, WwwFacebook, Telefon, Przedstawiciel,
    Kontakt, Fax, Email, Dostepnosc, OsobaDoKontaktu, TelefonOsobyKontaktowej,
    MailOsobyKontaktowej, OsobaOdbierajacaZywnosc, TelefonOsobyOdbierajacej,
    LiczbaBeneficjentow, Beneficjenci, Sieci, Bazarki, Machfit, FEPZ2024, Kategoria,
    RodzajPomocy, SposobUdzielaniaPomocy, WarunkiMagazynowe, HACCP, Sanepid,
    TransportOpis, TransportKategoria, Wniosek, UmowaZDn, UmowaRODO, KartyOrganizacjiData,
    OstatnieOdwiedzinyData
) VALUES (
    @Teczka, @IdentyfikatorEnova, @NIP, @Regon, @KrsNr, @FormaPrawna, @OPP,
    @NazwaOrganizacjiPodpisujacejUmowe, @AdresRejestrowy, @NazwaPlacowkiTrafiaZywnosc,
    @AdresPlacowkiTrafiaZywnosc, @GminaDzielnica, @Powiat, @NazwaOrganizacjiKsiegowanieDarowizn,
    @KsiegowanieAdres, @TelOrganProwadzacegoKsiegowosc, @WwwFacebook, @Telefon, @Przedstawiciel,
    @Kontakt, @Fax, @Email, @Dostepnosc, @OsobaDoKontaktu, @TelefonOsobyKontaktowej,
    @MailOsobyKontaktowej, @OsobaOdbierajacaZywnosc, @TelefonOsobyOdbierajacej,
    @LiczbaBeneficjentow, @Beneficjenci, @Sieci, @Bazarki, @Machfit, @FEPZ2024, @Kategoria,
    @RodzajPomocy, @SposobUdzielaniaPomocy, @WarunkiMagazynowe, @HACCP, @Sanepid,
    @TransportOpis, @TransportKategoria, @Wniosek, @UmowaZDn, @UmowaRODO, @KartyOrganizacjiData,
    @OstatnieOdwiedzinyData
)
"""

[<Fact>]
let ``Sample excel can be parsed to organizations and saved`` () =
    async {
        // Arrange
        let! db = con()
        let row = sampleOrgs.Rows |> Seq.head
        // Act
        let parsedRow = parse row
        do! db.Execute sqlSaveOrg parsedRow
        // Assert
        let! rowFromDb = db.Single<OrganizationRow>
                             "SELECT * FROM organizacje where Teczka = @Teczka"
                             {|Teczka = parsedRow.Teczka|}
        rowFromDb |> should equal parsedRow
        do! db.Execute "DELETE FROM organizacje WHERE Teczka = @Teczka" {|Teczka = parsedRow.Teczka|}
    }
