module Users.CompositionRoot

open Azure.Identity
open Users.Settings

type Dependencies = {
    ListUsers: Queries.ListUsers
    ListRoles: Queries.ListRoles
    FetchProfilePhoto: Queries.FetchProfilePhoto
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
    }
