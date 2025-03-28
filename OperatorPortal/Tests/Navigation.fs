module Navigation

open System.Net
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open FSharp.Data

[<Fact>]
let ``Navigations contains /ogranizations, /applications, /team links, /settings/password-change, /settings/csv-import`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate
        // Act
        let! response = api.GetAsync "/organizations"
        // Assert
        response.StatusCode |> should equal HttpStatusCode.OK
        let! document = response.HtmlContent()
        
        let navLinks = document.CssSelect "nav ul li a"
                       |> Seq.map(_.Attribute("href").Value())
                       |> Seq.toList
        navLinks |> should equal [ "/organizations"
                                   "/applications"
                                   "/team"
                                   "/login/password-change"
                                   "/import"
                                 ]    
    }