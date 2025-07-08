module Tests.Team.TeamViewing

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
    
[<Fact(Skip="Next")>]
let ``DELETE /team/users/{id} removes user`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync "/team/users"
        // Assert
        ()
    }