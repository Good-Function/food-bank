module Organizations

open System.Net
open Organizations.Funcs
open Tests
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open Organizations.Database.OrganizationsDao
open FSharp.Data

[<Fact>]
let ``POST /organizations/{id}/kontakty can edit organization``() =
    //save 
    ()

[<Fact>]
let ``/ogranizations/summaries displays organization's most important data `` () =
    task {
        // Arrange
        let! dbSummaries = readSummaries Tools.DbConnection.connectDb ("","")
        let dbSummaryTeczkaIds = dbSummaries |> List.map(fun summary -> $"%i{summary.Teczka}")
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync "/organizations/summaries"
        // Assert
        let! doc = response.HtmlContent()
        let summaries =
            doc.CssSelect "tbody tr"
            |> Seq.map(fun row -> row.Descendants "td" |> Seq.map(_.InnerText()))
            |> Seq.filter(fun row -> dbSummaryTeczkaIds |> List.contains (row |> Seq.head))
            |> Seq.toList
        response.StatusCode |> should equal HttpStatusCode.OK
        (dbSummaries, summaries) ||> List.iter2(fun dbSummary summary ->
            $"{dbSummary.Teczka}" |> should equal (summary |> Seq.item 0)
            dbSummary.NazwaPlacowkiTrafiaZywnosc |> should equal (summary |> Seq.item 1)
            dbSummary.AdresPlacowkiTrafiaZywnosc |> should equal (summary |> Seq.item 2)
            dbSummary.GminaDzielnica |> should equal (summary |> Seq.item 3)
            dbSummary.FormaPrawna |> should equal (summary |> Seq.item 4)
            dbSummary.Telefon |> should equal (summary |> Seq.item 5)
            dbSummary.Email |> should equal (summary |> Seq.item 6)
            dbSummary.Kontakt |> should equal (summary |> Seq.item 7)
            dbSummary.OsobaDoKontaktu |> should equal (summary |> Seq.item 8)
            dbSummary.TelefonOsobyKontaktowej |> should equal (summary |> Seq.item 9)
            dbSummary.Dostepnosc |> should equal (summary |> Seq.item 10)
            dbSummary.Kategoria |> should equal (summary |> Seq.item 11)
            $"%i{dbSummary.LiczbaBeneficjentow}" |> should equal (summary |> Seq.item 12)
            formatDate dbSummary.OstatnieOdwiedzinyData |> should equal (summary |> Seq.item 13)
             ) 
    }
    
[<Fact>]
let ``/ogranizations/{id} shows correct Identyfikatory, kontakty, dokumenty, adresy, adresy ksiegowosci, beneficjenci, zrodla zywnosci, warunku sections`` () =
    task {
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka}"
        // Assert
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()
        let sections = 
            doc.CssSelect "article"
            
        let extractSmallText (node: HtmlNode) = node.CssSelect("small") |> List.map _.InnerText()
            
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
            $"%i{organization.IdentyfikatorEnova}"
            $"%i{organization.NIP}"
            $"%i{organization.Regon}"
            organization.KrsNr
            organization.FormaPrawna
        ]
        kontakty[0..10] |> should equal [
            organization.WwwFacebook
            organization.Telefon
            organization.Przedstawiciel
            organization.Kontakt
            organization.Email
            organization.Dostepnosc
            organization.OsobaDoKontaktu
            organization.TelefonOsobyKontaktowej
            organization.MailOsobyKontaktowej
            organization.OsobaOdbierajacaZywnosc
            organization.TelefonOsobyOdbierajacej
        ]
        dokumenty[0..4] |> should equal [
            (organization.Wniosek |> Formatters.toDate)
            (organization.UmowaZDn |> Formatters.toDate)
            (organization.UmowaRODO |> Formatters.toDate)
            (organization.KartyOrganizacjiData |> Formatters.toDate)
            (organization.OstatnieOdwiedzinyData |> Formatters.toDate)
        ]
        adresy[0..5] |> should equal [
            organization.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
            organization.DaneAdresowe.AdresRejestrowy
            organization.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            organization.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
            organization.DaneAdresowe.GminaDzielnica
            organization.DaneAdresowe.Powiat
        ]
        adresyKsiegowosci[0..2] |> should equal [
            organization.NazwaOrganizacjiKsiegowanieDarowizn
            organization.KsiegowanieAdres
            organization.TelOrganProwadzacegoKsiegowosc
        ]
        beneficjenci[0..2] |> should equal [
            $"%i{organization.LiczbaBeneficjentow}"
            organization.Beneficjenci
        ]
        zrodlaZywnosci[0..3] |> should equal [
            (organization.Sieci |> Formatters.toTakNie)
            (organization.Bazarki |> Formatters.toTakNie)
            (organization.Machfit |> Formatters.toTakNie)
            (organization.FEPZ2024 |> Formatters.toTakNie)
        ]
        warunki[0..7] |> should equal [
            organization.Kategoria
            organization.RodzajPomocy
            organization.SposobUdzielaniaPomocy
            organization.WarunkiMagazynowe
            (organization.HACCP |> Formatters.toTakNie) 
            (organization.Sanepid |> Formatters.toTakNie)
            organization.TransportOpis
            organization.TransportKategoria
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/dane-adresowe/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka}/dane-adresowe/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input" |> List.map _.AttributeValue("value")
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
    
[<Fact>]
let ``GET /ogranizations/{id}/dane-adresowe returns fields from dane adresowe`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka}/dane-adresowe"
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