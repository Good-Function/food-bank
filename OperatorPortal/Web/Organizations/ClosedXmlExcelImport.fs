module Organizations.ClosedXmlExcelImport

open System
open System.IO
open ClosedXML.Excel
open FsToolkit.ErrorHandling
open Organizations.Application.CreateOrganizationCommands
open Organizations.Application.WriteModels

let trueValues = set ["tak"; "v"]
let falseValues = set ["nie"; "nie dotyczy"; ""]

let getCellText (index: int) (row: IXLRow) =
    row.Cell(index).GetValue<string>().Trim()

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
        
let mapRow (row: IXLRow): Result<Organization, string list> =
    let toInt64 (column: int) =
        let value = row |> getCellText column
        match Int64.TryParse(value) with
        | true, result -> Ok result
        | false, _ -> Error $"""Niepoprawna wartość: "%s{value}" w kolumnie [{expectedHeaders[column - 1]}]. Oczekiwana jest liczba."""

    let toBool column =
        match (row |> getCellText column).ToLowerInvariant() with
        | v when trueValues.Contains v -> Ok true
        | v when falseValues.Contains v -> Ok false
        | v -> Error $"""Niepoprawna wartość: "%s{v}" w kolumnie [{expectedHeaders[column - 1]}]. Oczekiwane jest 'tak' albo 'nie'."""

    let toDate (column: int) =
        let value = row |> getCellText column
        match value with
        | "" | "brak" -> Ok None
        | _ ->
            try 
                let date = row.Cell(column).GetDateTime()
                date |> DateOnly.FromDateTime |> Some |> Ok
            with
                | _ -> Error $"""Niepoprawna data: "%s{value}" w kolumnie [{expectedHeaders[column - 1]}]."""

    let parsedColumns = validation {
        let! teczka = 1 |> toInt64
        and! identyfikatorEnova = 2 |> toInt64
        and! nip = 3 |> toInt64
        and! krs = 3 |> toInt64
        and! regon = 5 |> toInt64
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
                  krs = krs
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
            KrsNr = columns.krs |> _.ToString()
            FormaPrawna = row |> getCellText 6
            OPP = columns.opp
            DaneAdresowe = {
                NazwaOrganizacjiPodpisujacejUmowe = row |> getCellText 8
                AdresRejestrowy = row |> getCellText 9
                NazwaPlacowkiTrafiaZywnosc = row |> getCellText 10
                AdresPlacowkiTrafiaZywnosc = row |> getCellText 11
                GminaDzielnica = row |> getCellText 12
                Powiat = row |> getCellText 13
            }
            AdresyKsiegowosci = {
                NazwaOrganizacjiKsiegowanieDarowizn = row |> getCellText 14
                KsiegowanieAdres = row |> getCellText 15
                TelOrganProwadzacegoKsiegowosc = row |> getCellText 16 
            }
            Kontakty = {
                WwwFacebook = row |> getCellText 17
                Telefon = row |> getCellText 18
                Przedstawiciel = row |> getCellText 19
                Kontakt = row |> getCellText 20
                Email = row |> getCellText 22
                Dostepnosc = row |> getCellText 23
                OsobaDoKontaktu = row |> getCellText 24
                TelefonOsobyKontaktowej = row |> getCellText 25
                MailOsobyKontaktowej = row |> getCellText 26
                OsobaOdbierajacaZywnosc = row |> getCellText 27
                TelefonOsobyOdbierajacej = row |> getCellText 28
            }
            Beneficjenci = {
                LiczbaBeneficjentow = row |> getCellText 29 |> Int32.Parse
                Beneficjenci = row |> getCellText 30
            }
            ZrodlaZywnosci = {
                Sieci = columns.sieci
                Bazarki = columns.bazarki
                Machfit = columns.machfit
                FEPZ2024 = columns.fepz2024
                OdbiorKrotkiTermin = columns.odbiorKrotkiTermin
                TylkoNaszMagazyn = columns.tylkoNaszMagazyn
            }
            WarunkiPomocy = {
                Kategoria = row |> getCellText 37
                RodzajPomocy = row |> getCellText 38
                SposobUdzielaniaPomocy = row |> getCellText 39
                WarunkiMagazynowe = row |> getCellText 40
                HACCP = columns.haccp
                Sanepid = columns.sanepid
                TransportOpis = row |> getCellText 43
                TransportKategoria = row |> getCellText 44
            }
            Dokumenty = {
                Wniosek = columns.wniosek 
                UmowaZDn = columns.umowaZDn 
                UmowaRODO = columns.umowaRODO
                KartyOrganizacjiData = columns.kartyOrganizacjiData
                OstatnieOdwiedzinyData = columns.ostatnieOdwiedzinyData
            }
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
        

let import: ParseOrganizations = fun stream ->
    result {
        let! parsedOrganizations = parseExcel stream
        let errors = parsedOrganizations |> List.choose (function Error err -> Some err | _ -> None)
        let orgs = parsedOrganizations |> List.choose (function Ok org -> Some org | _ -> None)
        return orgs, errors
    }
