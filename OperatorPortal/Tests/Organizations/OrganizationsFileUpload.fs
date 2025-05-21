module OrganizationsFileUpload

open System.IO
open System.Net
open System.Net.Http
open Organizations.Database.OrganizationsDao
open Organizations.Domain.Identifiers
open Tests.Arranger
open Tools.TestServer
open FsUnit.Xunit
open Xunit


// TODOMG
// 1. Finish the test - the file should be uploaded and view with link + delete should be returned (signed urls).
// 2. Remove BlobUpload.fs
// 3. Remove Azurite from testcontainers. It's flaky, write and use mock.
// 4. Implement same stuff for; wniosek /umowa /rodo /odwiedziny /upowaznienie-do-odbioru
// 5. Implement delete document.
[<Fact(Skip="todomg")>]
let ``PUT /organizations/{id}/wniosek returns 200 OK with link to document`` () =
    task {
        // Arrange
        let organization = AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let api = runTestApi() |> authenticate
        let fileContent = new ByteArrayContent(File.ReadAllBytes(Path.Combine(__SOURCE_DIRECTORY__, "doc.pdf")))
        use form = new MultipartFormDataContent()
        form.Add(fileContent, "doc", "doc.pdf")
        // Acts
        let! response = api.PutAsync($"/organizations/{organization.Teczka |> TeczkaId.unwrap}/dokumenty/wniosek", form)
        // Assert
        // response.StatusCode |> should equal HttpStatusCode.NotFound
        response.StatusCode |> should equal HttpStatusCode.OK
    }

