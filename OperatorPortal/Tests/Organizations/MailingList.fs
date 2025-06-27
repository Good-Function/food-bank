module Tests.Organizations.MailingList

open FSharp.Data
open Organizations.Domain.Identifiers
open Tools.TestServer
open Organizations.Database.OrganizationsDao
open Tests
open Xunit
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml

[<Fact>]
let ``Mailing list returns filtered mails``() =
    task {
        // Arrange
        let api = runTestApi() |> authenticate
        let org = Arranger.AnOrganization()
        do! org |> save Tools.DbConnection.connectDb
        // Act
        let! response = api.GetAsync $"/organizations/summaries/mailing-list?search={org.Teczka |> TeczkaId.unwrap}"
        // Assert
        let! mailingList = response.HtmlContent()
        let mailsText = mailingList.CssSelect("input").Head.Attribute("value").Value()
        mailsText |> should equal org.Kontakty.Email
    }