module Organizations.ClosedXmlExcelImport

open System
open System.Globalization
open System.IO
open Application.Commands
open ClosedXML.Excel
open FsToolkit.ErrorHandling
open Organizations.Database.OrganizationsDao

let trueValues = set ["tak"; "v"]
let falseValues = set ["nie"; "nie dotyczy"; ""]

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
    
let validateHeaders (headerRow: IXLRow): Result<unit, ImportError> =
    let actualHeaders = 
        headerRow.CellsUsed() 
        |> Seq.map _.GetValue<string>().Trim().ToLowerInvariant() 
        |> Seq.toList
    if actualHeaders <> expectedHeaders then
        Error <| ImportError.InvalidHeaders {|ActualHeaders = actualHeaders; ExpectedHeaders = expectedHeaders|}
    else
        Ok ()
        
let mapRow (row: IXLRow) =
    let getCellValue (index: int) =
        row.Cell(index).GetValue<string>().Trim()

    let toInt64 (column: int) =
        let value = getCellValue column
        match Int64.TryParse(value) with
        | true, result -> Ok result
        | false, _ -> Error $"""Niepoprawna wartość: "%s{value}" w kolumnie [{expectedHeaders[column - 1]}]. Oczekiwana jest liczba."""

    let toBool column =
        match (getCellValue column).ToLowerInvariant() with
        | v when trueValues.Contains v -> Ok true
        | v when falseValues.Contains v -> Ok false
        | v -> Error $"""Niepoprawna wartość: "%s{v}" w kolumnie {expectedHeaders[column - 1]}. Oczekiwane jest 'tak' albo 'nie'"""

    let toDate (column: int) = 
        let value = getCellValue column
        match value, DateTime.TryParseExact(value, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None) with
        | "", _ | "brak", _ -> Ok None
        | _, (true, result) ->  Ok (Some <| DateOnly.FromDateTime result)
        | _ -> Error $"""Niepoprawna data: "%s{value}" w kolumnie {expectedHeaders[column - 1]}."""

    let parsedColumns = validation {
        let! teczka = 1 |> toInt64
        and! identyfikatorEnova = 2 |> toInt64
        and! nip = 3 |> toInt64
        and! regon = 4 |> toInt64
        and! krsNr = 5 |> toInt64
        and! opp = 7 |> toBool
        and! sieci = 31 |> toBool
        and! odbiorKrotkiTermin = 32 |> toBool
        and! bazarki = 33 |> toBool
        and! machfit = 34 |> toBool
        and! tylkoNaszMagazyn = 35 |> toBool
        and! fepz2024 = 36 |> toBool
        and! haccp = 41 |> toBool
        and! sanepid = 42 |> toBool
        and! wniosek = 45 |> toDate
        and! umowaZDn = 46 |> toDate
        and! umowaRODO = 47 |> toDate
        and! kartyOrganizacjiData = 48 |> toDate
        and! ostatnieOdwiedzinyData = 49 |> toDate
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
            LiczbaBeneficjentow = getCellValue 29 |> Int32.Parse
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

let tryOpenWorkbook (stream: Stream) : Result<IXLWorksheet * XLWorkbook, ImportError> =
    try
        let workbook = new XLWorkbook(stream)
        let worksheet = workbook.Worksheet(1)
        Ok (worksheet, workbook)
    with
    | ex -> Error (ImportError.InvalidFile $"Failed to read Excel file: {ex.Message}")

let parseExcel (stream: Stream)=
    result {
        let! worksheet, workbook = tryOpenWorkbook stream
        use _ = workbook
        let headerRow = worksheet.FirstRowUsed()
        do! validateHeaders headerRow
        return worksheet.RowsUsed()
            |> Seq.takeWhile (fun row -> not (row.Cell(1).IsEmpty()))
            |> Seq.skip 1
            |> Seq.mapi (fun index row -> 
                match mapRow row with
                | Ok result -> Ok result
                | Error errors -> Error (index + 2, errors))
            |> Seq.toList
    }
        

let import: ImportOrganizations = fun stream ->
    result {
        let! parsedOrganizations = parseExcel stream
        let errors = parsedOrganizations |> List.choose (function Error err -> Some err | _ -> None)
        let orgs = parsedOrganizations |> List.choose (function Ok org -> Some org | _ -> None)
        return (orgs.Length, errors)
    }
