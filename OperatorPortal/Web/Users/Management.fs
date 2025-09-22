module Users.Management

open System
open System.Threading.Tasks
open Azure.Identity
open Microsoft.Graph
open Microsoft.Graph.Models
open Users.Domain

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
            if oldAssignment.ResourceId = Nullable(Guid(principal.Id)) then
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

let removeUser (credential: ClientSecretCredential) (userId: UserId) =
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
            if assignment.ResourceId = Nullable(Guid(principal.Id)) then
                do! client.Users.[userId.ToString()]
                        .AppRoleAssignments.[assignment.Id.ToString()]
                        .DeleteAsync()
    }

    
let inviteUser (credential:ClientSecretCredential): Commands.AddUser =
    fun newUser -> 
        task {
            use client = new GraphServiceClient(credential, scopes)
            let invitationRequest = 
                Invitation(
                    InvitedUserEmailAddress = newUser.Email,
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
    
let fetchPhoto (credential: ClientSecretCredential): Queries.FetchProfilePhoto =
    fun (userId: string) ->
        task {
            use client = new GraphServiceClient(credential, scopes)
            try
                let! photo = client.Users[userId].Photo.Content.GetAsync()
                return Some photo
            with
            | :? ODataErrors.ODataError as ex when ex.Error.Code = "ImageNotFound"->
                return None
        }
    
let fetchRoles (credential: ClientSecretCredential): Queries.ListRoles =
    fun () -> 
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

let fetchAppUsers (credential: ClientSecretCredential) : Queries.ListUsers =
    fun () ->
        task {
            use client = new GraphServiceClient(credential, scopes)

            let! principal =
                client
                    .ServicePrincipalsWithAppId("03241880-d8b0-408f-800e-1a0aec3e8746")
                    .GetAsync(fun config -> config.QueryParameters.Expand <- [| "AppRoleAssignedTo" |])
                    
            let assignments = principal.AppRoleAssignedTo
                              |> Seq.toList
                              |> List.filter(fun appRole -> appRole.AppRoleId.HasValue &&
                                                            appRole.PrincipalType = "User")
            let! users =
                assignments |> List.map(fun assignment ->
                    task {
                        let! user = client.Users[assignment.PrincipalId.Value.ToString()].GetAsync()
                        return {
                            Id = UserId user.Id
                            Mail = user.Mail
                            RoleId = assignment.AppRoleId.Value
                        }
                    }) |> Task.WhenAll
            return users |> Array.toList |> List.sortBy _.Mail
        }
