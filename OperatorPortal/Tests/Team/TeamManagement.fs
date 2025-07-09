module Team.TeamManagement

open Bogus
open Tools.FormDataBuilder
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open FSharp.Data

[<Fact>]
let ``/team displays team page`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync "/team"
        // Assert
        let! doc = response.HtmlContent()
        let headers =
            doc.CssSelect("th") |> List.map _.InnerText()
        headers |> should supersetOf ["Mail"; "Rola"]
    }
    
[<Fact>]
let ``/team/users displays team members and roles`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync "/team/users"
        // Assert
        let! doc = response.HtmlContent()
        let headers = doc.CssSelect("th") |> List.map(_.InnerText())
        let rows = doc.CssSelect("td") |> List.map(_.InnerText())
        let roles = doc.CssSelect("option") |> List.map(_.InnerText())
        headers |> should supersetOf ["Mail"; "Rola"]
        rows |> should contain "admin@bzsoswaw.pl"
        roles |> should supersetOf ["Reader"; "Editor"; "Admin"]
    }
    
[<Fact>]
let ``POST /team/users/{id} adds user`` () =
    task {
        // Arrange
        let emailToAdd = Faker().Person.Email
        let api = runTestApi() |> authenticate
        let data = formData {
            yield ("Email", emailToAdd)
        }
        // Act
        let! response = api.PostAsync("/team/users", data)
        // Assert
        (int response.StatusCode) |> should equal HttpStatusCodes.Created
        let! doc = response.HtmlContent()
        let rows = doc.CssSelect("td") |> List.map(_.InnerText())
        rows |> should contain emailToAdd
    }