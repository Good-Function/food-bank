module OrganizationEditing

open System
open System.Net
open Organizations.Templates.DetailsTemplate
open Tests
open Tools.FormDataBuilder
open Web.Organizations.Templates.Formatters
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
let ``PUT /ogranizations/{id}/dane-adresowe modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let randomStuff = Guid.NewGuid().ToString()
        let data = formData {
            yield ("NazwaOrganizacjiPodpisujacejUmowe", randomStuff)
            yield ("AdresRejestrowy", randomStuff)
            yield ("NazwaPlacowkiTrafiaZywnosc", randomStuff)
            yield ("AdresPlacowkiTrafiaZywnosc", randomStuff)
            yield ("GminaDzielnica", randomStuff)
            yield ("Powiat", randomStuff)
        }
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.PutAsync($"/organizations/{organization.Teczka}/dane-adresowe", data)
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
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka}/kontakty/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input" |> List.map _.AttributeValue("value")
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
        let data = formData {
            yield ("WwwFacebook", randomStuff)
            yield ("Telefon", randomStuff)
            yield ("Przedstawiciel", randomStuff)
            yield ("Kontakt", randomStuff)
            yield ("Email", randomStuff)
            yield ("Dostepnosc", randomStuff)
            yield ("OsobaDoKontaktu", randomStuff)
            yield ("TelefonOsobyKontaktowej", randomStuff)
            yield ("MailOsobyKontaktowej", randomStuff)
            yield ("OsobaOdbierajacaZywnosc", randomStuff)
            yield ("TelefonOsobyOdbierajacej", randomStuff)
        }
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.PutAsync($"/organizations/{organization.Teczka}/kontakty", data)
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
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka}/beneficjenci/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input" |> List.map _.AttributeValue("value")
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
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let expectedLiczbaBeneficjentow = organization.Beneficjenci.LiczbaBeneficjentow + 20 |> _.ToString()
        let expectedBeneficjenci = $"{Guid.NewGuid()}"
        let data = formData {
            yield ("LiczbaBeneficjentow", expectedLiczbaBeneficjentow)
            yield ("Beneficjenci", expectedBeneficjenci)
        }
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.PutAsync($"/organizations/{organization.Teczka}/beneficjenci", data)
        // Assert
        let! doc = response.HtmlContent()
        let dates =
            doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        dates |> should equal [
            expectedLiczbaBeneficjentow
            expectedBeneficjenci
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/dokumenty/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka}/dokumenty/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input" |> List.map _.AttributeValue("value")
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            organization.Dokumenty.Wniosek |> toInput
            organization.Dokumenty.UmowaZDn |> toInput
            organization.Dokumenty.UmowaRODO |> toInput
            organization.Dokumenty.KartyOrganizacjiData |> toInput
            organization.Dokumenty.OstatnieOdwiedzinyData |> toInput
        ]
        inputs.Length |> should equal 5
    }
    
    
[<Fact>]
let ``PUT /ogranizations/{id}/dokumenty modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        let expectedDate = Some <| DateOnly.FromDateTime(DateTime.Today)
        do! organization |> (save Tools.DbConnection.connectDb)
        let data = formData {
            yield ("Wniosek", expectedDate |> toInput)
            yield ("UmowaZDn", "")
            yield ("UmowaRODO", expectedDate |> toInput)
            yield ("KartyOrganizacjiData", expectedDate |> toInput)
            yield ("OstatnieOdwiedzinyData", expectedDate |> toInput)
        }
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.PutAsync($"/organizations/{organization.Teczka}/dokumenty", data)
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
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/zroda-zywnosci/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka}/zrodla-zywnosci/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input"
                |> List.filter _.HasAttribute("checked", "")
                |> List.map(fun input -> input.AttributeValue("value") |> Boolean.Parse)
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [
            organization.ZrodlaZywnosci.Sieci
            organization.ZrodlaZywnosci.Bazarki
            organization.ZrodlaZywnosci.Machfit
            organization.ZrodlaZywnosci.FEPZ2024
        ]
        inputs.Length |> should equal 4
    }
    
    
[<Fact>]
let ``PUT /ogranizations/{id}/zrodla-zywnosci modifies and returns updated data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let data = formData {
            yield ("Sieci", "true")
            yield ("Bazarki", "true")
            yield ("Machfit", "true")
            yield ("FEPZ2024", "true")
        }
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.PutAsync($"/organizations/{organization.Teczka}/zrodla-zywnosci", data)
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
        ]
    }
    
[<Fact>]
let ``GET /ogranizations/{id}/adresy-ksiegowosci/edit returns prefilled inputs to edit the data`` () =
    task {
        // Arrange
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync $"/organizations/{organization.Teczka}/adresy-ksiegowosci/edit"
        // Assert
        let! doc = response.HtmlContent()
        let inputs =
            doc.CssSelect "input" |> List.map _.AttributeValue("value")
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
        do! organization |> (save Tools.DbConnection.connectDb)
        let expectedText = $"{Guid.NewGuid()}"
        let data = formData {
            yield ("NazwaOrganizacjiKsiegowanieDarowizn", expectedText)
            yield ("KsiegowanieAdres", expectedText)
            yield ("TelOrganProwadzacegoKsiegowosc", expectedText)
        }
        // Arrange
        let api = runTestApi() |> authenticate "TestUser"
        // Act
        let! response = api.PutAsync($"/organizations/{organization.Teczka}/adresy-ksiegowosci", data)
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