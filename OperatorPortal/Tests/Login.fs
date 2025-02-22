module Login

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
let ``When unauthenticated user tries to access /applications then it is redirected to /login with ReturnUrl to /applications``
    ()
    =
    task {
        let api = runTestApi()
        let! response = api.GetAsync "/applications"
        response.StatusCode |> should equal HttpStatusCode.Found
        response.Headers.Location
            |> should equal (Uri "http://localhost/login?ReturnUrl=%2Fapplications")
    }

[<Fact>]
let ``User can log in, get the auth cookie and be redirected to default page /organizations`` () =
    task {
        // Arrange
        let api = runTestApi()
        let data = formData {
            yield ("Email", "test@test.test")
            yield ("Password", "password123")
        }
        // Act
        let! loginResponse = api.PostAsync("/login", data)
        // Assert
        let authCookie = loginResponse.Headers.GetValues("Set-Cookie") |> Seq.head
        let hxRedirectCookie = loginResponse.Headers.GetValues("HX-Redirect") |> Seq.head
        authCookie.StartsWith "authorization-cookie=" |> should equal true
        authCookie.EndsWith "httponly" |> should equal true
        hxRedirectCookie |> should equal "/organizations"
        loginResponse.StatusCode |> should equal HttpStatusCode.OK
    }
