module OrganizationsFromExcel.Learning_ClosedXMLReading

open System.IO
open ClosedXML.Excel
open Xunit
open FsUnit.Xunit

type OrganizationExcelRow =
    { Teczka: string
      IdentyfikatorEnova: string
      NIP: string
      Regon: string
      KrsNr: string
      FormaPrawna: string
      OPP: string
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
      Fax: string
      Email: string
      Dostepnosc: string
      OsobaDoKontaktu: string
      TelefonOsobyKontaktowej: string
      MailOsobyKontaktowej: string
      OsobaOdbierajacaZywnosc: string
      TelefonOsobyOdbierajacej: string
      LiczbaBeneficjentow: string
      Beneficjenci: string
      Sieci: string
      Bazarki: string
      Machfit: string
      FEPZ2024: string
      Kategoria: string
      RodzajPomocy: string
      SposobUdzielaniaPomocy: string
      WarunkiMagazynowe: string
      HACCP: string
      Sanepid: string
      TransportOpis: string
      TransportKategoria: string
      Wniosek: string
      UmowaZDn: string
      UmowaRODO: string
      KartyOrganizacjiData: string
      OstatnieOdwiedzinyData: string }

let expectedHeaders =
    [ "teczka"
      "Identyfikator ENOVA"
      "NIP"
      "regon"
      "krs/ nr w rejestrze"
      "Forma prawna"
      "OPP"
      "Nazwa organizacji, która podpisała umowę"
      "Adres rejestrowy"
      "Nazwa placówki do której trafia żywność"
      "Adres placówki, do której trafia żywność"
      "Gmina/ Dzielnica"
      "Powiat"
      "Nazwa organizacji na którą wystawiamy WZ (Księgowanie darowizn)"
      "Księgowanie adres"
      "Tel. organ prowadzącego księgowość"
      "www/ facebook"
      "Telefon"
      "Przedstawiciel"
      "Kontakt"
      "Fax"
      "e-mail"
      "dostępność"
      "Osoba do kontaktu"
      "telefon do osoby kontaktowej"
      "mail osoby kontaktowej"
      "Osoba odbierająca żywność"
      "telefon do osoby odbierającej"
      "Liczba beneficjentów"
      "Beneficjenci"
      "sieci"
      "Bazarki"
      "Machfit"
      "FEPŻ 2024"
      "Kategoria"
      "RODZAJ POMOCY"
      "Sposób udzielania pomocy"
      "Warunki magazynowe"
      "haccp"
      "SANEPID"
      "transport - opis"
      "transport - kategoria"
      "Wniosek"
      "Umowa z dnia"
      "Umowa RODO"
      "Karty organizacji data"
      "Ostatnie odwiedziny data" ]
    
let validateHeaders (headerRow: IXLRow): Result<unit, string> =
    let actualHeaders = 
        headerRow.CellsUsed() 
        |> Seq.map _.GetValue<string>().Trim() 
        |> Seq.toList
    if actualHeaders <> expectedHeaders then
        Error <| $"Header mismatch! Expected: %A{expectedHeaders}, but got: %A{actualHeaders}"
    else
        Ok ()

let mapRow (row: IXLRow) =
    let getCellValue (index: int) =
        let value = row.Cell(index).GetValue<string>()
        value

    { Teczka = getCellValue 1
      IdentyfikatorEnova = getCellValue 2
      NIP = getCellValue 3
      Regon = getCellValue 4
      KrsNr = getCellValue 5
      FormaPrawna = getCellValue 6
      OPP = getCellValue 7
      NazwaOrganizacjiPodpisujacejUmowe = getCellValue 8
      AdresRejestrowy = getCellValue 9
      NazwaPlacowkiTrafiaZywnosc = getCellValue 10
      AdresPlacowkiTrafiaZywnosc = getCellValue 11
      GminaDzielnica = getCellValue 12
      Powiat = getCellValue 13
      NazwaOrganizacjiKsiegowanieDarowizn = getCellValue 14
      KsiegowanieAdres = getCellValue 15
      TelOrganProwadzacegoKsiegowosc = getCellValue 16
      WwwFacebook = getCellValue 17
      Telefon = getCellValue 18
      Przedstawiciel = getCellValue 19
      Kontakt = getCellValue 20
      Fax = getCellValue 21
      Email = getCellValue 22
      Dostepnosc = getCellValue 23
      OsobaDoKontaktu = getCellValue 24
      TelefonOsobyKontaktowej = getCellValue 25
      MailOsobyKontaktowej = getCellValue 26
      OsobaOdbierajacaZywnosc = getCellValue 27
      TelefonOsobyOdbierajacej = getCellValue 28
      LiczbaBeneficjentow = getCellValue 29
      Beneficjenci = getCellValue 30
      Sieci = getCellValue 31
      Bazarki = getCellValue 32
      Machfit = getCellValue 33
      FEPZ2024 = getCellValue 34
      Kategoria = getCellValue 35
      RodzajPomocy = getCellValue 36
      SposobUdzielaniaPomocy = getCellValue 37
      WarunkiMagazynowe = getCellValue 38
      HACCP = getCellValue 39
      Sanepid = getCellValue 40
      TransportOpis = getCellValue 41
      TransportKategoria = getCellValue 42
      Wniosek = getCellValue 43
      UmowaZDn = getCellValue 44
      UmowaRODO = getCellValue 45
      KartyOrganizacjiData = getCellValue 46
      OstatnieOdwiedzinyData = getCellValue 47 }


let readExcelRows (stream: FileStream) =
    use workbook = new XLWorkbook(stream)
    let worksheet = workbook.Worksheet(1)
    let headerRow = worksheet.FirstRowUsed()

    let validation = validateHeaders headerRow

    worksheet.RowsUsed()
    |> Seq.skip 1 // Skip header row
    |> Seq.map mapRow
    |> Seq.toList

[<Fact>]
let ``Parsing excel file`` () =
    task {
        // Arrange
        use stream = File.OpenRead(Path.Combine(__SOURCE_DIRECTORY__, "bank.xlsx"))
        let rows = readExcelRows stream
        // Assert
        rows.Length |> should equal 12
    }
