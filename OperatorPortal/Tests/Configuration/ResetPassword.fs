module Tests.Configuration.ResetPassword

open System.Net
open FSharp.Data
open Tools.FormDataBuilder
open Xunit
open Tools.TestServer
open Tools.HttResponseMessageToHtml
open FsUnit.Xunit

[<Fact>]
let ``Reset password have fields to old and new password`` () =
    task {
        let api = runTestApi () |> authenticate "TestUser"
        // Act
        let! response = api.GetAsync "/settings/password-reset"
        // Assert
        let! doc = response.HtmlContent()
        response.StatusCode |> should equal HttpStatusCode.OK
        let labels = doc.CssSelect "label" |> List.map _.InnerText()
        labels |> should equal ["Stare hasło"; "Nowe hasło"; "Powtórz nowe hasło"]
    }
    
[<Fact>]
let ``Change password changes the password (wow!)`` () =
    task {
        let api = runTestApi () |> authenticate "TestUser"
        let passwordChange = formData {
            yield ("OldPassword", "password123")
            yield ("NewPassword", "password1234")
            yield ("NewPasswordConfirmation", "password1234")
        }
        // Act
        let! response = api.PostAsync("/settings/password-reset", passwordChange)
        // Assert
        //let! doc = response.HtmlContent()
        ()
    }
