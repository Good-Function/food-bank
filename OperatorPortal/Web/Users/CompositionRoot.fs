module Users.CompositionRoot

open Azure.Identity
open Users.Settings

type Dependencies = {
    ListUsers: Queries.ListUsers
    ListRoles: Queries.ListRoles
    FetchProfilePhoto: Queries.FetchProfilePhoto
    AddUser: Commands.AddUser
    DeleteUser: Commands.DeleteUser
    AssignRole: Commands.AssignRole
}

let build (azureAdSettings: AzureAdSettings) (settings: UsersSettings) =
    let credential = ClientSecretCredential(
        azureAdSettings.TenantId,
        azureAdSettings.ClientId,
        azureAdSettings.ClientSecret)
    {
        ListUsers = if settings.UseEntraMock
                    then ManagementMock.fetchAppUsers
                    else Management.fetchAppUsers credential
        ListRoles = if settings.UseEntraMock
                    then ManagementMock.fetchRoles
                    else Management.fetchRoles credential
        FetchProfilePhoto = if settings.UseEntraMock
                            then ManagementMock.fetchPhoto
                            else Management.fetchPhoto credential
        AddUser = if settings.UseEntraMock
                  then ManagementMock.inviteUser
                  else Management.inviteUser credential
        DeleteUser = if settings.UseEntraMock
                     then ManagementMock.removeUser
                     else Management.removeUser credential
        AssignRole = if settings.UseEntraMock
                     then ManagementMock.assignRoleToUser
                     else Management.assignRoleToUser credential
    }
