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
        let title =
            doc.CssSelect("h1").Head.InnerText()
        title |> should equal "Zespół"
    }