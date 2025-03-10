module OrganizationEditing

open System
open System.Net
open Tests
open Tools.FormDataBuilder
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
let ``PUT /ogranizations/{id}/dane-adresowe returns modifies and returns updated data`` () =
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
let ``PUT /ogranizations/{id}/kontakt returns modifies and returns updated data`` () =
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