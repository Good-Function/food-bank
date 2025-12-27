module ImportFromExcel

open System.IO
open System.Net
open System.Net.Http
open Tools.TestServer
open Tools.HttResponseMessageToHtml
open FSharp.Data
open FsUnit.Xunit
open Xunit

[<Fact>]
let ``POST /organizations/import/upload returns bad request when file is not xlsx`` () =
    task {
        // Arrange
        let api = runTestApi()
        let! token = getAntiforgeryToken api "/organizations/import"
        let fileBytes = File.ReadAllBytes(Path.Combine(__SOURCE_DIRECTORY__, "notExcel.txt"))
        let fileContent = new ByteArrayContent(fileBytes)
        use form = new MultipartFormDataContent()
        form.Add(new StringContent(token), "__RequestVerificationToken")
        form.Add(fileContent, "file", "notExcel.txt")
        // Acts
        let! response = api.PostAsync("/organizations/import/upload", form)
        // Assert
        response.StatusCode |> should equal HttpStatusCode.BadRequest
        let! doc = response.HtmlContent()
        let error = doc.CssSelect "#file-error" |> List.head
        error.InnerText() |> should haveSubstring "Failed to read Excel file: File contains corrupted data"
    }

[<Fact>]
let ``POST /organizations/import/upload returns info with how many rows were important along with errors.`` () =
    task {
        // Arrange
        let api = runTestApi()
        let! token = getAntiforgeryToken api "/organizations/import"
        let fileBytes = File.ReadAllBytes(Path.Combine(__SOURCE_DIRECTORY__, "bank.xlsx"))
        let fileContent = new ByteArrayContent(fileBytes)
        use form = new MultipartFormDataContent()
        form.Add(new StringContent(token), "__RequestVerificationToken")
        form.Add(fileContent, "file", "bank.xlsx")
        // Acts
        let! response = api.PostAsync("/organizations/import/upload", form)
        // Assert
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()
        let ringNode = doc.CssSelect ".ring-chart" |> List.head
        let value = (ringNode.Attribute "data-value").Value()
        let total = (ringNode.Attribute "data-max").Value()
        let errors = doc.CssSelect "li" |> List.map(_.InnerText())
        value |> should equal "9"
        total |> should equal "11"
        errors |> should equal [
            """Niepoprawna data: "wysłana 26.06.2024" w kolumnie [umowa z dnia]."""
            """Niepoprawna wartość: "nd" w kolumnie [krs/ nr w rejestrze]."""
        ]
    }

