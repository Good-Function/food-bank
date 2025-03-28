module Login.ResetPassword

open System
open System.Net
open FSharp.Data
open Login.Database.LoginDao
open Login.Database
open Login.Domain
open Login.SignInHandler
open Tools.DbConnection
open Tools.FormDataBuilder
open Xunit
open Tools.TestServer
open Tools.HttResponseMessageToHtml
open FsUnit.Xunit

[<Fact>]
let ``Change password have fields to old and new password`` () =
    task {
        let api = runTestApi () |> authenticate
        // Act
        let! response = api.GetAsync "/login/password-change"
        // Assert
        let! doc = response.HtmlContent()
        response.StatusCode |> should equal HttpStatusCode.OK
        let labels = doc.CssSelect "label" |> List.map _.InnerText()
        labels |> should equal ["Stare hasło"; "Nowe hasło"; "Powtórz nowe hasło"]
    }
    
[<Fact>]
let ``Change password changes the password (wow!)`` () =
    // This is test relies on the fact that dev-only user with Id 0 exists.
    // This is user is inserted into users by sample.sql in Login vertical slice.
    // Note that the password is not needed for user with Id 0.
    // Fake authentication middleware does this.
    task {
        let testUserId = 0
        let oldPassword = Guid.NewGuid().ToString()
        let oldPasswordHash = oldPassword |> computeHash |> Password.create
        let expectedPassword = Guid.NewGuid().ToString()
        do! PasswordDao.changePassword connectDb {UserId = testUserId; NewPassword = oldPasswordHash}
        let api = runTestApi () |> authenticate
        let passwordChange = formData {
            yield ("OldPassword", oldPassword)
            yield ("NewPassword", expectedPassword)
            yield ("NewPasswordConfirmation", expectedPassword)
        }
        // Act
        let! response = api.PostAsync("/login/password-change", passwordChange)
        // Assert
        let! doc = response.HtmlContent()
        let headers = doc.Elements "h1" |> List.map _.InnerText()
        headers |> should contain "Hasło zostało zmienione✅"
    }
    
[<Fact>]
let ``Change password shows error when old password is not correct`` () =
    task {
        let api = runTestApi () |> authenticate
        let passwordChange = formData {
            yield ("OldPassword", Guid.NewGuid().ToString())
            yield ("NewPassword", "whatever")
            yield ("NewPasswordConfirmation", "whatever")
        }
        // Act
        let! response = api.PostAsync("/login/password-change", passwordChange)
        // Assert
        let! doc = response.HtmlContent()
        let error = doc.CssSelect("#OldPassword-error") |> List.map _.InnerText()
        error |> should contain "Stare hasło jest niepoprawne."
    }
    
[<Fact>]
let ``Change password shows error when new passowrd confirmation doesn't match new password`` () =
    task {
        let! user = readUserBy connectDb 0
        let api = runTestApi () |> authenticate
        let passwordChange = formData {
            yield ("OldPassword", user.Password |> Password.value)
            yield ("NewPassword", "blabla1")
            yield ("NewPasswordConfirmation", "blabla2")
        }
        // Act
        let! response = api.PostAsync("/login/password-change", passwordChange)
        // Assert
        let! doc = response.HtmlContent()
        let error = doc.CssSelect("#NewPasswordConfirmation-error") |> List.map _.InnerText()
        error |> should contain "Hasło nie zgadza się."
    }

