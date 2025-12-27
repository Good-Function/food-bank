module Login.Login

open System
open System.Net
open Tools.FormDataBuilder
open Tools.TestServer
open Xunit
open FSharp.Control
open FsUnit.Xunit

[<Fact>]
let ``By default user is redirected to /organizations page`` () =
    task {
        let api = runTestApi()
        let! response = api.GetAsync "/"
        response.StatusCode |> should equal HttpStatusCode.Found
        response.Headers.Location |> should equal (Uri("/organizations"))
    }

[<Fact>]
let ``User can log in, get the auth cookie and be redirected to default page /organizations`` () =
    task {
        // Arrange
        let api = runTestApi()
        // Act
        let! loginResponse = postFormWithToken api "/login" "/login" [
            ("Email", "admin@admin.pl")
            ("Password", "f00d!")
        ]
        // Assert
        let authCookie = loginResponse.Headers.GetValues("Set-Cookie") |> Seq.head
        let hxRedirectCookie = loginResponse.Headers.GetValues("HX-Redirect") |> Seq.head
        authCookie.EndsWith "httponly" |> should equal true
        hxRedirectCookie |> should equal "/organizations"
        loginResponse.StatusCode |> should equal HttpStatusCode.OK
    }
