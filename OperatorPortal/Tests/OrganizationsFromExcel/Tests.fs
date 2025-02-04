module Tests.OrganizationsFromExcel.Tests

open Xunit
open FsUnit.Xunit
open Organizations.Database.csvLoader
open Organizations.Database.OrganizationRow
open PostgresPersistence.DapperFsharp

[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__

let sampleOrgs = Orgs.GetSample()

let sqlSaveOrg = """
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
) VALUES (
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

[<Fact>]
let ``Sample excel can be parsed to organizations and saved`` () =
    async {
        // Arrange
        let! db = Tools.DbConnection.connectDb()
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
