module OrganizationEditing

open System
open System.Net
open Organizations.Domain.Identifiers
open Organizations.Domain.Organization
open Oxpecker.ViewEngine
open Tests
open Tools.FormDataBuilder
open Organizations.Templates.Formatters
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open Organizations.Database.OrganizationsDao
open FSharp.Data
    
[<Fact>]
let ``GET /ogranizations/{id}/dane-adresowe/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi()
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka |> TeczkaId.unwrap}/dane-adresowe/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input"
            |> List.filter (fun input -> input.AttributeValue("name") <> "__RequestVerificationToken")
            |> List.map _.AttributeValue("value")
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
let ``PUT /ogranizations/{id}/dane-adresowe modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let randomStuff = Guid.NewGuid().ToString()
        let api = runTestApi() |> authenticate "Editor"
        let teczka = organization.Teczka |> TeczkaId.unwrap
        // Act
        let! response = putFormWithToken api ($"/organizations/{teczka}/dane-adresowe/edit") ($"/organizations/{teczka}/dane-adresowe") [
            ("NazwaOrganizacjiPodpisujacejUmowe", randomStuff)
            ("AdresRejestrowy", randomStuff)
            ("NazwaPlacowkiTrafiaZywnosc", randomStuff)
            ("AdresPlacowkiTrafiaZywnosc", randomStuff)
            ("GminaDzielnica", randomStuff)
            ("Powiat", randomStuff)
        ]
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
        ]
        inputs.Length |> should equal 6
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/kontakty/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> save Tools.DbConnection.connectDb
        // Arrange
        let api = runTestApi()
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka |> TeczkaId.unwrap}/kontakty/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input"
            |> List.filter (fun input -> input.AttributeValue("name") <> "__RequestVerificationToken")
            |> List.map _.AttributeValue("value")
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
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
        inputs.Length |> should equal 11
    }
    
    
[<Fact>]
let ``PUT /ogranizations/{id}/kontakty modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let randomStuff = Guid.NewGuid().ToString()
        let api = runTestApi() |> authenticate "Editor"
        let teczka = organization.Teczka |> TeczkaId.unwrap
        // Act
        let! response = putFormWithToken api ($"/organizations/{teczka}/kontakty/edit") ($"/organizations/{teczka}/kontakty") [
            ("WwwFacebook", randomStuff)
            ("Telefon", randomStuff)
            ("Przedstawiciel", randomStuff)
            ("Kontakt", randomStuff)
            ("Email", randomStuff)
            ("Dostepnosc", randomStuff)
            ("OsobaDoKontaktu", randomStuff)
            ("TelefonOsobyKontaktowej", randomStuff)
            ("MailOsobyKontaktowej", randomStuff)
            ("OsobaOdbierajacaZywnosc", randomStuff)
            ("TelefonOsobyOdbierajacej", randomStuff)
        ]
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
            randomStuff
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/beneficjenci/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi()
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka |> TeczkaId.unwrap}/beneficjenci/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input"
            |> List.filter (fun input -> input.AttributeValue("name") <> "__RequestVerificationToken")
            |> List.map _.AttributeValue("value")
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            $"{organization.Beneficjenci.LiczbaBeneficjentow}"
            organization.Beneficjenci.Beneficjenci
        ]
    }
    
[<Fact>]
let ``PUT /ogranizations/{id}/beneficjenci modifies and returns updated data`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate "Editor"
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let expectedLiczbaBeneficjentow = organization.Beneficjenci.LiczbaBeneficjentow + 20 |> _.ToString()
        let expectedBeneficjenci = $"{Guid.NewGuid()}"
        let teczka = organization.Teczka |> TeczkaId.unwrap
        // Act
        let! response = putFormWithToken api ($"/organizations/{teczka}/beneficjenci/edit") ($"/organizations/{teczka}/beneficjenci") [
            ("LiczbaBeneficjentow", expectedLiczbaBeneficjentow)
            ("Beneficjenci", expectedBeneficjenci)
        ]
        // Assert
        let! doc = response.HtmlContent()
        let beneficjenciValues =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        beneficjenciValues |> should equal [
            expectedLiczbaBeneficjentow
            expectedBeneficjenci
        ]
    }
    
[<Fact(Skip="todomg")>]
let ``GET /ogranizations/{id}/dokumenty/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi()
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka |> TeczkaId.unwrap}/dokumenty/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input"
            |> List.filter (fun input -> input.AttributeValue("name") <> "__RequestVerificationToken")
            |> List.map _.AttributeValue("value")
        response.StatusCode |> should equal HttpStatusCode.OK
        let map (doc: Document) = doc.Date
        inputs |> should equal [
            organization.Dokumenty.Wniosek |> map |> toInput
            organization.Dokumenty.Umowa |> map |> toInput
            organization.Dokumenty.Rodo |> map |> toInput
            organization.Dokumenty.Odwiedziny |> map |> toInput
            organization.Dokumenty.UpowaznienieDoOdbioru |> map |> toInput
        ]
    }
    
    
[<Fact(Skip="todomg")>]
let ``PUT /ogranizations/{id}/dokumenty modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        let expectedDate = Some <| DateOnly.FromDateTime DateTime.Today
        do! organization |> save Tools.DbConnection.connectDb
        let data = formData {
            yield "Wniosek", expectedDate |> toInput
            yield "UmowaZDn", ""
            yield "UmowaRODO", expectedDate |> toInput
            yield "KartyOrganizacjiData", expectedDate |> toInput
            yield "OstatnieOdwiedzinyData", expectedDate |> toInput
            yield "DataUpowaznieniaDoOdbioru", expectedDate |> toInput
        }
        // Arrange
        let api = runTestApi()
        // Act
        let! response = api.PutAsync($"/organizations/{organization.Teczka |> TeczkaId.unwrap}/dokumenty", data)
        // Assert
        let! doc = response.HtmlContent()
        let dates =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        dates |> should equal [
            expectedDate |> toDisplay
            None |> toDisplay
            expectedDate |> toDisplay
            expectedDate |> toDisplay
            expectedDate |> toDisplay
            expectedDate |> toDisplay
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/zroda-zywnosci/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi()
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka |> TeczkaId.unwrap}/zrodla-zywnosci/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input"
                |> List.filter (fun input -> input.AttributeValue("name") <> "__RequestVerificationToken")
                |> List.filter _.HasAttribute("checked", "")
                |> List.map(fun input -> input.AttributeValue("value") |> Boolean.Parse)
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            organization.ZrodlaZywnosci.Sieci
            organization.ZrodlaZywnosci.Bazarki
            organization.ZrodlaZywnosci.Machfit
            organization.ZrodlaZywnosci.FEPZ2024
            organization.ZrodlaZywnosci.OdbiorKrotkiTermin
            organization.ZrodlaZywnosci.TylkoNaszMagazyn
        ]
        inputs.Length |> should equal 6
    }
    
    
[<Fact>]
let ``PUT /ogranizations/{id}/zrodla-zywnosci modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> save Tools.DbConnection.connectDb
        let api = runTestApi() |> authenticate "Editor"
        let teczka = organization.Teczka |> TeczkaId.unwrap
        // Act
        let! response = putFormWithToken api ($"/organizations/{teczka}/zrodla-zywnosci/edit") ($"/organizations/{teczka}/zrodla-zywnosci") [
            ("Sieci", "true")
            ("Bazarki", "true")
            ("Machfit", "true")
            ("FEPZ2024", "true")
            ("OdbiorKrotkiTermin", "true")
            ("TylkoNaszMagazyn", "true")
        ]
        // Assert
        let! doc = response.HtmlContent()
        let takNie =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        takNie |> should equal [
            "Tak"
            "Tak"
            "Tak"
            "Tak"
            "Tak"
            "Tak"
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/adresy-ksiegowosci/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate "Editor"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka |> TeczkaId.unwrap}/adresy-ksiegowosci/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input"
            |> List.filter (fun input -> input.AttributeValue("name") <> "__RequestVerificationToken")
            |> List.map _.AttributeValue("value")
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            organization.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
            organization.AdresyKsiegowosci.KsiegowanieAdres
            organization.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
        ]
    }
    
[<Fact>]
let ``PUT /ogranizations/{id}/adresy-ksiegowosci modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> save Tools.DbConnection.connectDb
        let expectedText = $"{Guid.NewGuid()}"
        let api = runTestApi() |> authenticate "Editor"
        let teczka = organization.Teczka |> TeczkaId.unwrap
        // Act
        let! response = putFormWithToken api ($"/organizations/{teczka}/adresy-ksiegowosci/edit") ($"/organizations/{teczka}/adresy-ksiegowosci") [
            ("NazwaOrganizacjiKsiegowanieDarowizn", expectedText)
            ("KsiegowanieAdres", expectedText)
            ("TelOrganProwadzacegoKsiegowosc", expectedText)
        ]
        // Assert
        let! doc = response.HtmlContent()
        let dates =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        dates |> should equal [
            expectedText
            expectedText
            expectedText
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/warunki-pomocy/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi()
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka |> TeczkaId.unwrap}/warunki-pomocy/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input"
            |> List.filter (fun input -> input.AttributeValue("name") <> "__RequestVerificationToken")
            |> List.filter(fun elem -> elem.AttributeValue("type") <> "radio")
            |> List.map _.AttributeValue("value")
        let radios =
            doc.CssSelect "input"
                |> List.filter (fun input -> input.AttributeValue("name") <> "__RequestVerificationToken")
                |> List.filter _.HasAttribute("checked", "")
                |> List.map(fun input -> input.AttributeValue("value") |> Boolean.Parse)
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            organization.WarunkiPomocy.Kategoria
            organization.WarunkiPomocy.RodzajPomocy
            organization.WarunkiPomocy.SposobUdzielaniaPomocy
            organization.WarunkiPomocy.WarunkiMagazynowe
            organization.WarunkiPomocy.TransportOpis
            organization.WarunkiPomocy.TransportKategoria
        ]
        radios |> should equal [
            organization.WarunkiPomocy.HACCP
            organization.WarunkiPomocy.Sanepid
        ]
    }
    
[<Fact>]
let ``PUT /ogranizations/{id}/warunki-pomocy modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> save Tools.DbConnection.connectDb
        let expectedText = $"{Guid.NewGuid()}"
        let api = runTestApi() |> authenticate "Editor"
        let teczka = organization.Teczka |> TeczkaId.unwrap
        // Act
        let! response = putFormWithToken api ($"/organizations/{teczka}/warunki-pomocy/edit") ($"/organizations/{teczka}/warunki-pomocy") [
            ("Kategoria", expectedText)
            ("RodzajPomocy", expectedText)
            ("SposobUdzielaniaPomocy", expectedText)
            ("WarunkiMagazynowe", expectedText)
            ("HACCP", "true")
            ("Sanepid", "true")
            ("TransportOpis", expectedText)
            ("TransportKategoria", expectedText)
        ]
        // Assert
        let! doc = response.HtmlContent()
        let warunkiPomocy =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        warunkiPomocy |> should equal [
            expectedText
            expectedText
            expectedText
            expectedText
            "Tak"
            "Tak"
            expectedText
            expectedText
        ]
    }