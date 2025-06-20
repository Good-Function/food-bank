module OrganizationViewing

open System
open System.Net
open Organizations.Domain.FormaPrawna
open Organizations.Domain.Identifiers
open Organizations.Domain.Organization
open Tests
open Organizations.Templates.Formatters
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open Organizations.Database.OrganizationsDao
open FSharp.Data

[<Fact>]
let ``/ogranizations/summaries?search filters out and displays organization's most important data`` () =
    task {
        // Arrange
        let org =  Arranger.AnOrganization()
        do! org |> (save Tools.DbConnection.connectDb)
        let! dbSummaries = readSummaries Tools.DbConnection.connectDb {
            SearchTerm = org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            SortBy = None
            Filters = []
        }
        let dbSummaryTeczkaIds = dbSummaries |> List.map(fun summary -> $"%i{summary.Teczka}")
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync $"/organizations/summaries?search={org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc}"
        // Assert
        let! doc = response.HtmlContent()
        let summaries =
            doc.CssSelect "tr"
            |> Seq.map(fun row -> row.Descendants "td" |> Seq.map(_.InnerText()))
            |> Seq.filter(fun row -> row |> Seq.length > 0)
            |> Seq.filter(fun row -> dbSummaryTeczkaIds |> List.contains (row |> Seq.head))
            |> Seq.toList
        printfn "%A" summaries
        response.StatusCode |> should equal HttpStatusCode.OK
        (dbSummaries, summaries) ||> List.iter2(fun dbSummary summary ->
            $"{dbSummary.Teczka}" |> should equal (summary |> Seq.item 0)
            dbSummary.NazwaPlacowkiTrafiaZywnosc |> should equal (summary |> Seq.item 1)
            dbSummary.AdresPlacowkiTrafiaZywnosc |> should equal (summary |> Seq.item 2)
            dbSummary.GminaDzielnica |> should equal (summary |> Seq.item 3)
            dbSummary.FormaPrawna |> should equal (summary |> Seq.item 4)
            // dbSummary.Telefon |> should equal (summary |> Seq.item 5)
            // dbSummary.Email |> should equal (summary |> Seq.item 6)
            // dbSummary.Kontakt |> should equal (summary |> Seq.item 7)
            // dbSummary.OsobaDoKontaktu |> should equal (summary |> Seq.item 8)
            // dbSummary.TelefonOsobyKontaktowej |> should equal (summary |> Seq.item 9)
            // dbSummary.Dostepnosc |> should equal (summary |> Seq.item 10)
            dbSummary.Kategoria |> should equal (summary |> Seq.item 5)
            dbSummary.Beneficjenci |> should equal (summary |> Seq.item 6)
            $"%i{dbSummary.LiczbaBeneficjentow}" |> should equal (summary |> Seq.item 7)
            dbSummary.OstatnieOdwiedzinyData |> toDisplay |> should equal (summary |> Seq.item 8)
             ) 
    }

[<Theory>]
[<InlineData("asc")>]
[<InlineData("desc")>]
let ``/ogranizations/summaries?search=xxx&sort=OstatnieOdwiedziny&dir=asc filters out and displays organizations sorted data`` (dir: string) =
    task {
        // Arrange
        let id = Guid.NewGuid().ToString()
        let orgVisitedToday =  Arranger.AnOrganization()
                               |> Arranger.setOstatnieOdwiedziny(DateOnly.FromDateTime(DateTime.Today))
                               |> Arranger.setNazwaPlacowki $"{id}org1"
        let orgVisitedYesterday = Arranger.AnOrganization()
                                    |> Arranger.setOstatnieOdwiedziny(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)))
                                    |> Arranger.setNazwaPlacowki $"{id}org1"
        do! orgVisitedToday |> (save Tools.DbConnection.connectDb)
        do! orgVisitedYesterday |> (save Tools.DbConnection.connectDb)
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync $"/organizations/summaries?search={id}&sort=OstatnieOdwiedzinyData&dir={dir}"
        let! headersResponse = api.GetAsync "/organizations"
        // Assert
        let! doc = response.HtmlContent()
        let! headersDoc = headersResponse.HtmlContent()
        let indexOfOstatnieOdwiedziny = headersDoc.CssSelect("th")
                                        |> List.map _.InnerText()
                                        |> List.indexed
                                        |> List.find(fun(_, text) -> text.Contains "Odwiedzono")
                                        |> fst
        let summaries =
            doc.CssSelect "tr"
            |> List.map(fun tr -> tr.Descendants "td" |> Seq.map(_.InnerText()) |> Seq.toList)
            |> Seq.filter(fun tr -> tr.Length > 0)
            |> Seq.toList
        response.StatusCode |> should equal HttpStatusCode.OK
        summaries.Length |> should equal 2
        let dateFrom1stRow = summaries[0][indexOfOstatnieOdwiedziny] |> DateOnly.Parse
        let dateFrom2ndRow = summaries[1][indexOfOstatnieOdwiedziny] |> DateOnly.Parse
        if dir = "asc" then
            dateFrom1stRow |> should lessThan dateFrom2ndRow
        else
            dateFrom1stRow |> should greaterThan dateFrom2ndRow
    }
    
[<Fact>]
let ``/ogranizations/{id} shows correct Identyfikatory, kontakty, dokumenty, adresy, adresy ksiegowosci, beneficjenci, zrodla zywnosci, warunku sections`` () =
    task {
        let organization = Arranger.AnOrganization()
        let teczkaid = organization.Teczka |> TeczkaId.unwrap
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync $"/organizations/{teczkaid}"
        // Assert
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()
        let sections = 
            doc.CssSelect "article"
            
        let extractSmallText (node: HtmlNode) = node.CssSelect("small") |> List.map _.InnerText()
        let map (doc: Document) = doc.Date
            
        let identyfikatory,
            kontakty,
            dokumenty,
            adresy,
            adresyKsiegowosci,
            beneficjenci,
            zrodlaZywnosci,
            warunki= (
                sections[0] |> extractSmallText,
                sections[1] |> extractSmallText,
                sections[2] |> extractSmallText,
                sections[3] |> extractSmallText,
                sections[4] |> extractSmallText,
                sections[5] |> extractSmallText,
                sections[6] |> extractSmallText,
                sections[7] |> extractSmallText
                )
        identyfikatory[0..4] |> should equal [
            $"{organization.IdentyfikatorEnova}"
            organization.NIP |> Nip.unwrap
            organization.Regon |> Regon.unwrap
            organization.FormaPrawna.Rejestracja |> function
                | WRejestrzeKRS krs -> Krs.unwrap krs
                | PozaRejestrem nr -> nr
            organization.FormaPrawna.Nazwa
        ]
        kontakty[0..10] |> should equal [
            organization.Kontakty.WwwFacebook
            organization.Kontakty.Telefon
            organization.Kontakty.Przedstawiciel
            organization.Kontakty.Kontakt
            organization.Kontakty.Email
            organization.Kontakty.Dostepnosc
            organization.Kontakty.OsobaDoKontaktu
            organization.Kontakty.TelefonOsobyKontaktowej
            organization.Kontakty.MailOsobyKontaktowej
            organization.Kontakty.OsobaOdbierajacaZywnosc
            organization.Kontakty.TelefonOsobyOdbierajacej
        ]
        // dokumenty[0..4] |> should equal [
        //     (organization.Dokumenty.Wniosek |> map |> toDisplay)
        //     (organization.Dokumenty.Umowa |> map |> toDisplay)
        //     (organization.Dokumenty.Rodo |> map |> toDisplay)
        //     (organization.Dokumenty.Odwiedziny |> map |> toDisplay)
        // ]
        adresy[0..5] |> should equal [
            organization.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
            organization.DaneAdresowe.AdresRejestrowy
            organization.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            organization.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
            organization.DaneAdresowe.GminaDzielnica
            organization.DaneAdresowe.Powiat
        ]
        adresyKsiegowosci[0..2] |> should equal [
            organization.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
            organization.AdresyKsiegowosci.KsiegowanieAdres
            organization.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
        ]
        beneficjenci[0..2] |> should equal [
            $"%i{organization.Beneficjenci.LiczbaBeneficjentow}"
            organization.Beneficjenci.Beneficjenci
        ]
        zrodlaZywnosci[0..5] |> should equal [
            (organization.ZrodlaZywnosci.Sieci |> Formatters.toTakNie)
            (organization.ZrodlaZywnosci.Bazarki |> Formatters.toTakNie)
            (organization.ZrodlaZywnosci.Machfit |> Formatters.toTakNie)
            (organization.ZrodlaZywnosci.FEPZ2024 |> Formatters.toTakNie)
            (organization.ZrodlaZywnosci.OdbiorKrotkiTermin |> Formatters.toTakNie)
            (organization.ZrodlaZywnosci.TylkoNaszMagazyn |> Formatters.toTakNie)
        ]
        warunki[0..7] |> should equal [
            organization.WarunkiPomocy.Kategoria
            organization.WarunkiPomocy.RodzajPomocy
            organization.WarunkiPomocy.SposobUdzielaniaPomocy
            organization.WarunkiPomocy.WarunkiMagazynowe
            (organization.WarunkiPomocy.HACCP |> Formatters.toTakNie) 
            (organization.WarunkiPomocy.Sanepid |> Formatters.toTakNie)
            organization.WarunkiPomocy.TransportOpis
            organization.WarunkiPomocy.TransportKategoria
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/dane-adresowe returns fields from dane adresowe`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        let teczkaid = organization.Teczka |> TeczkaId.unwrap
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync $"/organizations/{teczkaid}/dane-adresowe"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            organization.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
            organization.DaneAdresowe.AdresRejestrowy
            organization.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            organization.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
            organization.DaneAdresowe.GminaDzielnica
            organization.DaneAdresowe.Powiat
        ]
        inputs.Length |> should equal 6
    }