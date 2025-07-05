module Learning.Entra

open System
open Azure.Identity
open Xunit
open FsUnit.Xunit
open Users

let tenantId = "57495abd-aab1-4bdc-8f4e-077d9448831b"
let clientId = "03241880-d8b0-408f-800e-1a0aec3e8746"
let clientSecret = Environment.GetEnvironmentVariable("AzureAd__ClientSecret")

let credential = ClientSecretCredential(tenantId, clientId, clientSecret)

[<Fact(Skip="Learning entra")>]
let ``List roles``() =
    task {
        let! roles = Management.fetchRoles credential
        roles |> List.map(_.Name) |> should subsetOf ["Admin"; "Reader"; "Editor"]
    }
    
[<Fact(Skip="Learning entra")>]
let ``List app users``() =
    task {
        let! roles = Management.fetchAppUsers credential
        roles |> List.map(_.Mail) |> should supersetOf ["Marcin Golenia"]
    }
    
[<Fact(Skip="Learning entra")>]
let ``Assign role to user``() =
    task {
        let! roles = Management.fetchRoles credential
        let editorRole = roles |> List.find(fun role -> role.Name = "Reader")
        let! users = Management.fetchAppUsers credential
        let user = users |> List.find(fun user -> user.Mail = "Marcin Golenia")
        
        do! Management.assignRoleToUser credential user.Id editorRole.Id
    }

[<Fact(Skip="Learning entra")>]
let ``Delete user``() =
    task {
        do! Management.removeUser credential (Guid "eff8d726-2f0c-4f22-9c5c-6f398592cf3b")
    }
    
[<Fact(Skip="Learning entra")>]
let ``Invite user``() =
    task {
        do! Management.inviteUser credential "marcingolenia@gmail.com"
        printf "%s" "ok"// eff8d726-2f0c-4f22-9c5c-6f398592cf3b
    }