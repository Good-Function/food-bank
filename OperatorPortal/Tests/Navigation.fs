module Navigation

open System.IO
open System.Net
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open FSharp.Data

[<Fact>]
let ``Navigations contains /ogranizations, /applications, /team links`` () =
    task {
        // Arrange
        let api = runTestApi().CreateClient()
        api.DefaultRequestHeaders.Add(Authentication.FakeAuthenticationHeader, "TestUser")
        // Act
        let! response = api.GetAsync "/organizations"
        let! stuff = response.Content.ReadAsStringAsync()
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "test.txt"), stuff)
        response.StatusCode |> should equal 100
        stuff |> should equal "teradascie"
        // Assert
        let! document = response.HtmlContent()
        
        let navLinks = document.CssSelect "nav ul li a"
                       |> Seq.map(_.Attribute("href").Value())
                       |> Seq.toList
        navLinks |> should equal [ "/organizations"; "/applications"; "/team" ]    
        response.StatusCode |> should equal HttpStatusCode.OK
    }