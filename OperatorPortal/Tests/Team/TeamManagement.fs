module Team.TeamManagement

open System
open Bogus
open Users
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Tools.HttResponseMessageToHtml
open FSharp.Data

[<Fact>]
let ``/team displays team page`` () =
    task {
        // Arrange
        let api = runTestApi()
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
        let api = runTestApi()
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
        let api = runTestApi() |> authenticate "Admin"
        // Act
        let! response = postFormWithToken api "/team" "/team/users" [
            ("Email", emailToAdd)
        ]
        // Assert
        (int response.StatusCode) |> should equal HttpStatusCodes.Created
        let! doc = response.HtmlContent()
        let rows = doc.CssSelect("td") |> List.map(_.InnerText())
        rows |> should contain emailToAdd
    }
    
[<Fact>]
let ``PUT /team/users/{id}/role/{roleId} assigns role`` () =
    task {
        // Arrange
        let api = runTestApi() |> authenticate "Admin"
        let editorRole = ManagementMock.editor
        let userIdToChangeRole = ManagementMock.users.Head.Id
        // Act
        let! response = putFormWithToken api "/team/users" $"/team/users/{userIdToChangeRole}/roles" [
            ("RoleId", editorRole.Id.ToString())
        ]
        // Assert
        (int response.StatusCode) |> should equal HttpStatusCodes.OK
        let! doc = response.HtmlContent()
        let selectedRole = doc.CssSelect("option[selected]").Head.InnerText()
        selectedRole |> should equal editorRole.Name
    }
    
[<Fact>]
let ``DELETE /team/users/{id} removes user`` () =
    task {
        // Arrange
        let userToDelete: Domain.User = {
            Id = Guid.NewGuid()
            Mail = "admin@bzsoswaw.pl"
            RoleId = ManagementMock.roles[0].Id }
        ManagementMock.users <- userToDelete :: ManagementMock.users
        let api = runTestApi() |> authenticate "Admin"
        // Act
        let! response = api.DeleteAsync($"/team/users/{userToDelete.Id}")
        // Assert
        (int response.StatusCode) |> should equal HttpStatusCodes.OK
        ManagementMock.users
            |> List.contains(userToDelete)
            |> should equal false
    }
    
[<Theory>]
[<InlineData("Read")>]
[<InlineData("Editor")>]
let ``Delete /team/users/{id} returns Forbidden (403)`` role =
     task {
         let api = runTestApi() |> authenticate role
         let! response = api.DeleteAsync("/team/users/10")
         (int response.StatusCode) |> should equal HttpStatusCodes.Forbidden
     }