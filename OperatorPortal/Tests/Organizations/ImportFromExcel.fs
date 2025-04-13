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
let ``GET /organizations/import/upload returns bad request when file is not xlsx`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate
        let fileBytes = File.ReadAllBytes(Path.Combine(__SOURCE_DIRECTORY__, "notExcel.txt"))
        let fileContent = new ByteArrayContent(fileBytes)
        use form = new MultipartFormDataContent()
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
let ``GET /organizations/import/upload returns success message when xlsx is correct`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate
        let fileBytes = File.ReadAllBytes(Path.Combine(__SOURCE_DIRECTORY__, "bank.xlsx"))
        let fileContent = new ByteArrayContent(fileBytes)
        use form = new MultipartFormDataContent()
        form.Add(fileContent, "file", "bank.xlsx")
        // Acts
        let! response = api.PostAsync("/organizations/import/upload", form)
        // Assert
        response.StatusCode |> should equal HttpStatusCode.OK
        // todomg
        ()
    }

