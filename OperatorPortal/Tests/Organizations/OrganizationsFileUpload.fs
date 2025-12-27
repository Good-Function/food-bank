module OrganizationsFileUpload

open System
open System.IO
open System.Net
open System.Net.Http
open FSharp.Data
open Organizations.Database.OrganizationsDao
open Tools.HttResponseMessageToHtml
open Organizations.Domain.Identifiers
open Organizations.Templates.Formatters
open Tests.Arranger
open Tools.TestServer
open FsUnit.Xunit
open Xunit

[<Fact>]
let ``PUT /organizations/{id}/dokumenty returns 200 OK with link to document`` () =
    task {
        // Arrange
        let organization = AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let teczka = organization.Teczka |> TeczkaId.unwrap
        let! token = getAntiforgeryToken api $"/organizations/{teczka}/dokumenty/edit"
        let wniosekDate = DateOnly.FromDateTime(DateTime.Today)|> Some
        let umowaDate = DateOnly.FromDateTime(DateTime.Today)|> Some
        let rodoDate = DateOnly.FromDateTime(DateTime.Today)|> Some
        let odwiedzinyDate = DateOnly.FromDateTime(DateTime.Today)|> Some
        let upowaznienieDate = DateOnly.FromDateTime(DateTime.Today)|> Some
        let file = File.ReadAllBytes(Path.Combine(__SOURCE_DIRECTORY__, "doc.pdf"))
        use form = new MultipartFormDataContent()
        form.Add(new StringContent(token), "__RequestVerificationToken")
        form.Add(new ByteArrayContent(file), "Wniosek", "wniosek.pdf")
        form.Add(new ByteArrayContent(file), "Umowa", "umowa.pdf")
        form.Add(new ByteArrayContent(file), "RODO", "rodo.pdf")
        form.Add(new ByteArrayContent(file), "Odwiedziny", "odwiedziny.pdf")
        form.Add(new ByteArrayContent(file), "UpowaznienieDoOdbioru", "upo.pdf")
        form.Add(new StringContent(wniosekDate |> toInput), "WniosekDate")
        form.Add(new StringContent(umowaDate |> toInput), "UmowaDate")
        form.Add(new StringContent(rodoDate |> toInput), "RODODate")
        form.Add(new StringContent(odwiedzinyDate |> toInput), "OdwiedzinyDate")
        form.Add(new StringContent(upowaznienieDate |> toInput), "UpowaznienieDoOdbioruDate")
        // Acts
        let! changeDocsResponse = api.PutAsync($"/organizations/{teczka}/dokumenty", form)
        // Assert
        let! docsResponse = api.GetAsync($"/organizations/{organization.Teczka |> TeczkaId.unwrap}/dokumenty")
        let! docsHtml = docsResponse.HtmlContent()
        let rows = docsHtml.CssSelect("tbody tr") |> List.map(_.InnerText())
        docsResponse.StatusCode |> should equal HttpStatusCode.OK
        changeDocsResponse.StatusCode |> should equal HttpStatusCode.OK
        rows |> should equal [
            $"wniosek.pdfWniosek{wniosekDate |> toDisplay}"
            $"umowa.pdfUmowa{umowaDate |> toDisplay}"
            $"odwiedziny.pdfOdwiedziny{odwiedzinyDate |> toDisplay}"
            $"rodo.pdfRodo{rodoDate |> toDisplay}"
            $"upo.pdfUpowaznienie do odbioru{upowaznienieDate |> toDisplay}"
        ]
    }

