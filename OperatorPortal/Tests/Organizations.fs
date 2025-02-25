module Organizations

open System.Net
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
        let! dbSummaries = readSummaries Tools.DbConnection.connectDb ""
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
             ) 
    }
    
[<Fact>]
let ``/ogranizations/{id} displays Identyfikatory, ... 's data's`` () =
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
            
        let identyfikatorySection,
            kontaktySection,
            dokumentySection,
            adresSection,
            adresKsiegowosciSection,
            beneficjenciSection,
            zrodlaZywnosciSection,
            warunkiSection= (
                sections[0],
                sections[1],
                sections[2],
                sections[3],
                sections[4],
                sections[5],
                sections[6],
                sections[7]
                )
        let identyfikatory = (identyfikatorySection.CssSelect "small") |> List.map(_.InnerText())
        let kontakty = (kontaktySection.CssSelect "small") |> List.map(_.InnerText())
        let dokumenty = (dokumentySection.CssSelect "small") |> List.map(_.InnerText())
        let adresy = (adresSection.CssSelect "small") |> List.map(_.InnerText())
        let adresyKsiegowosci = (adresKsiegowosciSection.CssSelect "small") |> List.map(_.InnerText())
        let beneficjenci = (beneficjenciSection.CssSelect "small") |> List.map(_.InnerText())
        let zrodlaZywnosci = (zrodlaZywnosciSection.CssSelect "small") |> List.map(_.InnerText())
        let warunki = (warunkiSection.CssSelect "small") |> List.map(_.InnerText())
        identyfikatory[0] |> should equal $"%i{organization.IdentyfikatorEnova}"
        identyfikatory[1] |> should equal $"%i{organization.NIP}"
        identyfikatory[2] |> should equal $"%i{organization.Regon}"
        identyfikatory[3] |> should equal organization.KrsNr
        identyfikatory[4] |> should equal organization.FormaPrawna
        kontakty[0] |> should equal organization.WwwFacebook
        dokumenty[0] |> should equal (organization.Wniosek.Value.ToString("dd.MM.yyyy", System.Globalization.CultureInfo("pl-PL")))
        adresy[0] |> should equal organization.NazwaOrganizacjiPodpisujacejUmowe
        adresyKsiegowosci[0] |> should equal organization.NazwaOrganizacjiKsiegowanieDarowizn
        beneficjenci[0] |> should equal $"%i{organization.LiczbaBeneficjentow}"
        warunki[0] |> should equal organization.Kategoria
        identyfikatory.Length |> should be (greaterThan 0)
    }