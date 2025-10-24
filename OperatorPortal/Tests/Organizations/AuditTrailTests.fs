module AuditTrailTests

open System
open System.Net
open Organizations.Domain.Identifiers
open Organizations.Domain.Organization
open Oxpecker.ViewEngine
open Tests
open Tools.FormDataBuilder
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open Organizations.Database.OrganizationsDao
open FSharp.Data

let shouldBeNowWithin (tolerance: TimeSpan) (actualString: string) =
    let actual = DateTime.Parse(actualString)
    let diff = (actual - DateTime.Now).Duration()
    diff |> should be (lessThan tolerance)

[<Fact>]
let ``Audit stores only changed fields and returns audit trail by related entity id and kind`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate "Editor"
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let expectedLiczbaBeneficjentow = organization.Beneficjenci.LiczbaBeneficjentow + 20 |> _.ToString()
        let data = formData {
            yield "LiczbaBeneficjentow", expectedLiczbaBeneficjentow
            yield "Beneficjenci", organization.Beneficjenci.Beneficjenci
        }
        // Act
        let! modificationResponse = api.PutAsync($"/organizations/{organization.Teczka |> TeczkaId.unwrap}/beneficjenci", data)
        // Assert
        let! auditTrailResponse = api.GetAsync($"organizations/{organization.Teczka |> TeczkaId.unwrap}/audit-trail?kind=beneficjenci")
        modificationResponse.StatusCode |> should equal HttpStatusCode.OK
        auditTrailResponse.StatusCode |> should equal HttpStatusCode.OK
        let! doc = auditTrailResponse.HtmlContent()
        let cells = doc.CssSelect("tbody td") |> List.map _.InnerText()
        cells[0] |> should equal Authentication.userName
        cells[1] |> shouldBeNowWithin (TimeSpan.FromSeconds(5.0))
        cells[2] |> should equal "LiczbaBeneficjentow"
        cells[3] |> should equal $"{organization.Beneficjenci.LiczbaBeneficjentow}"
        cells[4] |> should equal $"{organization.Beneficjenci.LiczbaBeneficjentow + 20}"
    }