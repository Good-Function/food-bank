module Navigation

open System.Net
open AngleSharp.Html.Parser
open Xunit
open TestServer
open FsUnit.Xunit

[<Fact>]
let ``Navigations contains /ogranizations, /applications, /team links`` () =
    task {
        // Arrange
        let api = runTestApi().CreateClient()
        api.DefaultRequestHeaders.Add("Authorization", "TestUser")
        // Act
        let! response = api.GetAsync "/organizations"
        // Assert
        let! document = response.Content.ReadAsStringAsync()
        let doc = HtmlParser().ParseDocument document
        let navLinks =
            doc.QuerySelectorAll "nav ul li a"
            |> Seq.map _.GetAttribute("href")
            |> Seq.toList
        navLinks |> should equal [ "/organizations"; "/applications"; "/team" ]    
        response.StatusCode |> should equal HttpStatusCode.OK
    }