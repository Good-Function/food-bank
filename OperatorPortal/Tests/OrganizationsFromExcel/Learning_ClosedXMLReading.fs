module OrganizationsFromExcel.Learning_ClosedXMLReading

open System
open System.Globalization
open System.IO
open ClosedXML.Excel
open Xunit
open FsUnit.Xunit
open Organizations.Database.OrganizationsDao
open FsToolkit.ErrorHandling


let trueValues = set ["tak"; "v"]
let falseValues = set ["nie"; "nie dotyczy"; ""]

let getExcelColumnName number =
    let rec computeColumnName num acc =
        if num <= 0 then acc
        else
            let modulo = (num - 1) % 26
            let char = char (65 + modulo) 
            computeColumnName ((num - 1) / 26) (string char + acc)
    computeColumnName number ""

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
      "Odbior krotki termin"
      "Bazarki"
      "Machfit"
      "Tylko nasz magazyn"
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
      "Ostatnie odwiedziny data" ] |> List.map(_.ToLowerInvariant())
    
let validateHeaders (headerRow: IXLRow): Result<unit, string> =
    let actualHeaders = 
        headerRow.CellsUsed() 
        |> Seq.map _.GetValue<string>().Trim().ToLowerInvariant() 
        |> Seq.toList
    if actualHeaders <> expectedHeaders then
        Error <| $"Header mismatch! Expected: %A{expectedHeaders}, but got: %A{actualHeaders}"
    else
        Ok ()

let mapRow (row: IXLRow) =
    let getCellValue (index: int) =
        row.Cell(index).GetValue<string>().Trim()

    let columntToInt64 (column: int) =
        let value = getCellValue column
        match Int64.TryParse(value) with
        | true, result -> Ok result
        | false, _ -> Error $"Invalid value: %s{value} at column {expectedHeaders[column - 1]}. Should be number."

    let columnToBool column =
        match (getCellValue column).ToLowerInvariant() with
        | v when trueValues.Contains v -> Ok true
        | v when falseValues.Contains v -> Ok false
        | v -> Error $"""Invalid boolean: "%s{v}" at column {expectedHeaders[column - 1]}. Should be 'tak' or 'nie'"""

    let columnToDate (column: int) = 
        let value = getCellValue column
        match value, DateTime.TryParse(value, CultureInfo.InvariantCulture) with
        | "", _ | "brak", _ -> Ok None
        | _, (true, result) ->  Ok (Some <| DateOnly.FromDateTime result)
        | _ -> Error $"""Invalid date: "%s{value}" at column {expectedHeaders[column - 1]}."""

    let parsedColumns = validation {
        let! teczka = 1 |> columntToInt64
        and! identyfikatorEnova = 2 |> columntToInt64
        and! nip = 3 |> columntToInt64
        and! regon = 4 |> columntToInt64
        and! krsNr = 4 |> columntToInt64
        and! opp = 7 |> columnToBool
        and! sieci = 31 |> columnToBool
        and! bazarki = 33 |> columnToBool
        and! machfit = 34 |> columnToBool
        and! fepz2024 = 36 |> columnToBool
        and! odbiorKrotkiTermin = 32 |> columnToBool
        and! tylkoNaszMagazyn = 35 |> columnToBool
        and! haccp = 41 |> columnToBool
        and! sanepid = 42 |> columnToBool
        and! wniosek = 45 |> columnToDate
        and! umowaZDn = 46 |> columnToDate
        and! umowaRODO = 47 |> columnToDate
        and! kartyOrganizacjiData = 48 |> columnToDate
        and! ostatnieOdwiedzinyData = 49 |> columnToDate
        return {|
                  teczka = teczka
                  identyfikatorEnova = identyfikatorEnova
                  nip = nip
                  regon = regon
                  krsNr = krsNr
                  opp = opp
                  sieci = sieci
                  bazarki = bazarki
                  machfit = machfit
                  fepz2024 = fepz2024
                  odbiorKrotkiTermin = odbiorKrotkiTermin
                  tylkoNaszMagazyn = tylkoNaszMagazyn
                  haccp = haccp
                  sanepid = sanepid
                  wniosek = wniosek
                  umowaZDn = umowaZDn
                  umowaRODO = umowaRODO
                  kartyOrganizacjiData = kartyOrganizacjiData
                  ostatnieOdwiedzinyData = ostatnieOdwiedzinyData
                  |}
    }
    result {
        let! columns = parsedColumns
        return {
            Teczka = columns.teczka
            IdentyfikatorEnova = columns.identyfikatorEnova
            NIP = columns.nip
            Regon = columns.regon
            KrsNr = getCellValue 5
            FormaPrawna = getCellValue 6
            OPP = columns.opp
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
            Email = getCellValue 22
            Dostepnosc = getCellValue 23
            OsobaDoKontaktu = getCellValue 24
            TelefonOsobyKontaktowej = getCellValue 25
            MailOsobyKontaktowej = getCellValue 26
            OsobaOdbierajacaZywnosc = getCellValue 27
            TelefonOsobyOdbierajacej = getCellValue 28
            LiczbaBeneficjentow = getCellValue 29 |> System.Int32.Parse
            Beneficjenci = getCellValue 30
            Sieci = columns.sieci
            Bazarki = columns.bazarki
            Machfit = columns.machfit
            FEPZ2024 = columns.fepz2024
            OdbiorKrotkiTermin = columns.odbiorKrotkiTermin
            TylkoNaszMagazyn = columns.tylkoNaszMagazyn
            Kategoria = getCellValue 37
            RodzajPomocy = getCellValue 38
            SposobUdzielaniaPomocy = getCellValue 39
            WarunkiMagazynowe = getCellValue 40
            HACCP = columns.haccp
            Sanepid = columns.sanepid
            TransportOpis = getCellValue 43
            TransportKategoria = getCellValue 44
            Wniosek = columns.wniosek
            UmowaZDn = columns.umowaZDn
            UmowaRODO = columns.umowaRODO
            KartyOrganizacjiData = columns.kartyOrganizacjiData
            OstatnieOdwiedzinyData = columns.ostatnieOdwiedzinyData
        }
    }

let readExcelRows (stream: FileStream) =
    use workbook = new XLWorkbook(stream)
    let worksheet = workbook.Worksheet(1)
    let headerRow = worksheet.FirstRowUsed()

    let validation = validateHeaders headerRow

    worksheet.RowsUsed()
    |> Seq.takeWhile (fun row -> not (row.Cell(1).IsEmpty()))
    |> Seq.skip 1 // Skip header row
    |> Seq.mapi (fun index row -> 
        match mapRow row with
        | Ok result -> Ok result
        | Error errors -> Error ($"Errors in Row {index + 1}", errors))
    |> Seq.toList

[<Fact>]
let ``Parsing excel file`` () =
    task {
        // Arrange
        use stream = File.OpenRead(Path.Combine(__SOURCE_DIRECTORY__, "bank.xlsx"))
        let rows = readExcelRows stream
        // Assert
        rows.Length |> should equal 11s
    }
