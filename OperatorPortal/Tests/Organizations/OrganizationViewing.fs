module OrganizationViewing

open System.Net
open Organizations.Domain.Identifiers
open Tests
open Organizations.Templates.Formatters
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open Organizations.Database.OrganizationsDao
open FSharp.Data

[<Fact>]
let ``/ogranizations/summaries displays organization's most important data `` () =
    task {
        // Arrange
        let org =  Arranger.AnOrganization()
        do! org |> (save Tools.DbConnection.connectDb)
        let! dbSummaries = readSummaries Tools.DbConnection.connectDb org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
        let dbSummaryTeczkaIds = dbSummaries |> List.map(fun summary -> $"%i{summary.Teczka}")
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync $"/organizations/summaries?search={org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc}"
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
            organization.KrsNr |> Krs.unwrap
            organization.FormaPrawna
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
        dokumenty[0..4] |> should equal [
            (organization.Dokumenty.Wniosek |> toDisplay)
            (organization.Dokumenty.UmowaZDn |> toDisplay)
            (organization.Dokumenty.UmowaRODO |> toDisplay)
            (organization.Dokumenty.KartyOrganizacjiData |> toDisplay)
            (organization.Dokumenty.OstatnieOdwiedzinyData |> toDisplay)
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