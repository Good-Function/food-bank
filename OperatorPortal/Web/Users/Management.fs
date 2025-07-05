module Users.Management

open System
open System.Threading.Tasks
open Azure.Identity
open Microsoft.Graph
open Microsoft.Graph.Models

type UserId = Guid

type Role =
    { Id: Guid
      Name: string
      Description: string }

type User =
    { Id: UserId
      Mail: string
      RoleId: Guid option }

let scopes = [| "https://graph.microsoft.com/.default" |]

let assignRoleToUser (credential: ClientSecretCredential) (userId: UserId) (appRoleId: Guid) =
    task {
        use client = new GraphServiceClient(credential, scopes)
        let! principal =
            client
                .ServicePrincipalsWithAppId("03241880-d8b0-408f-800e-1a0aec3e8746")
                .GetAsync()
        
        let! oldAssignments = 
            client.Users.[userId.ToString()]
                .AppRoleAssignments
                .GetAsync()
                
        for oldAssignment in oldAssignments.Value do
            if oldAssignment.ResourceId = Guid(principal.Id) then
                do! client.Users.[userId.ToString()]
                        .AppRoleAssignments.[oldAssignment.Id.ToString()]
                        .DeleteAsync()

        let newAssigment =
            AppRoleAssignment(
                PrincipalId = userId,
                ResourceId = Guid(principal.Id),
                AppRoleId = Nullable(appRoleId)
            )
            
        let! _ = client.Users.[userId.ToString()].AppRoleAssignments.PostAsync newAssigment
        ()
    }

let removeUser (credential: ClientSecretCredential) (userId: Guid) =
    task {
        use client = new GraphServiceClient(credential, scopes)
        let! assignments =
            client.Users.[userId.ToString()]
                .AppRoleAssignments
                .GetAsync()
                
        let! principal =
            client
                .ServicePrincipalsWithAppId("03241880-d8b0-408f-800e-1a0aec3e8746")
                .GetAsync()

        for assignment in assignments.Value do
            if assignment.ResourceId = Guid(principal.Id) then
                do! client.Users.[userId.ToString()]
                        .AppRoleAssignments.[assignment.Id.ToString()]
                        .DeleteAsync()
    }

    
let inviteUser (credential:ClientSecretCredential) (mail: string) : Task =
    task {
        use client = new GraphServiceClient(credential, scopes)
        let invitationRequest = 
            Invitation(
                InvitedUserEmailAddress = mail,
                InviteRedirectUrl = "https://operator-portal.bluemeadow-0985b16b.polandcentral.azurecontainerapps.io/",
                SendInvitationMessage = true
            )
        let! invitation = client.Invitations.PostAsync(invitationRequest)
        
        let! principal =
            client
                .ServicePrincipalsWithAppId("03241880-d8b0-408f-800e-1a0aec3e8746")
                .GetAsync()
                
        let readerRole = 
            principal.AppRoles
            |> Seq.find (fun role -> role.Value = "Reader")
            
        let assignment = 
            AppRoleAssignment(
                PrincipalId = Guid invitation.InvitedUser.Id,
                ResourceId = Guid principal.Id,
                AppRoleId = readerRole.Id
            )
        let! _ = client.Users.[invitation.InvitedUser.Id].AppRoleAssignments.PostAsync(assignment)
        ()
    }
    
let fetchRoles (credential: ClientSecretCredential): Task<Role list> =
    task {
        use client = new GraphServiceClient(credential, scopes)
        let! principal =
            client
                .ServicePrincipalsWithAppId("03241880-d8b0-408f-800e-1a0aec3e8746")
                .GetAsync(fun config -> config.QueryParameters.Expand <- [| "AppRoleAssignedTo" |])

        return principal.AppRoles |> Seq.toList |> List.map(fun role -> {
            Id = role.Id.Value
            Name = role.DisplayName
            Description = role.Description
        })
    }

let fetchAppUsers (credential: ClientSecretCredential) : Task<User list> =
    task {
        use client = new GraphServiceClient(credential, scopes)

        let! principal =
            client
                .ServicePrincipalsWithAppId("03241880-d8b0-408f-800e-1a0aec3e8746")
                .GetAsync(fun config -> config.QueryParameters.Expand <- [| "AppRoleAssignedTo" |])
                
        let assignments = principal.AppRoleAssignedTo |> Seq.toList

        return
            assignments
            |> List.map (fun user ->
                { Id = user.PrincipalId.Value
                  Mail = user.PrincipalDisplayName
                  RoleId = user.AppRoleId |> Option.ofNullable })
    }
