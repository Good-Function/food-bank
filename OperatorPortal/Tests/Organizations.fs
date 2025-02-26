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
        kontakty[1] |> should equal organization.Telefon
        kontakty[2] |> should equal organization.Przedstawiciel
        kontakty[3] |> should equal organization.Kontakt
        kontakty[4] |> should equal organization.Email
        kontakty[5] |> should equal organization.Dostepnosc
        kontakty[6] |> should equal organization.OsobaDoKontaktu
        kontakty[7] |> should equal organization.TelefonOsobyKontaktowej
        kontakty[8] |> should equal organization.MailOsobyKontaktowej
        kontakty[9] |> should equal organization.OsobaOdbierajacaZywnosc
        kontakty[10] |> should equal organization.TelefonOsobyOdbierajacej
        dokumenty[0] |> should equal (organization.Wniosek |> Formatters.toDate)
        dokumenty[1] |> should equal (organization.UmowaZDn |> Formatters.toDate)
        dokumenty[2] |> should equal (organization.UmowaRODO |> Formatters.toDate)
        dokumenty[3] |> should equal (organization.KartyOrganizacjiData |> Formatters.toDate)
        dokumenty[4] |> should equal (organization.OstatnieOdwiedzinyData |> Formatters.toDate)
        adresy[0] |> should equal organization.NazwaOrganizacjiPodpisujacejUmowe
        adresy[1] |> should equal organization.AdresRejestrowy
        adresy[2] |> should equal organization.NazwaPlacowkiTrafiaZywnosc
        adresy[3] |> should equal organization.AdresPlacowkiTrafiaZywnosc
        adresy[4] |> should equal organization.GminaDzielnica
        adresy[5] |> should equal organization.Powiat
        adresyKsiegowosci[0] |> should equal organization.NazwaOrganizacjiKsiegowanieDarowizn
        adresyKsiegowosci[1] |> should equal organization.KsiegowanieAdres
        adresyKsiegowosci[2] |> should equal organization.TelOrganProwadzacegoKsiegowosc
        beneficjenci[0] |> should equal $"%i{organization.LiczbaBeneficjentow}"
        beneficjenci[1] |> should equal organization.Beneficjenci
        zrodlaZywnosci[0] |> should equal (organization.Sieci |> Formatters.toTakNie)
        zrodlaZywnosci[1] |> should equal (organization.Bazarki |> Formatters.toTakNie)
        zrodlaZywnosci[2] |> should equal (organization.Machfit |> Formatters.toTakNie)
        zrodlaZywnosci[3] |> should equal (organization.FEPZ2024 |> Formatters.toTakNie)
        warunki[0] |> should equal organization.Kategoria
        warunki[1] |> should equal organization.RodzajPomocy
        warunki[2] |> should equal organization.SposobUdzielaniaPomocy
        warunki[3] |> should equal organization.WarunkiMagazynowe
        warunki[4] |> should equal (organization.HACCP |> Formatters.toTakNie) 
        warunki[5] |> should equal (organization.Sanepid |> Formatters.toTakNie)
        warunki[6] |> should equal organization.TransportOpis
        warunki[7] |> should equal organization.TransportKategoria
        identyfikatory.Length |> should be (greaterThan 0)
    }