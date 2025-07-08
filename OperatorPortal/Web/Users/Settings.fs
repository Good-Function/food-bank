module Users.Settings

[<CLIMutable>]
type AzureAdSettings = {
    Instance: string
    Domain: string
    TenantId: string
    ClientId: string
    CallbackPath: string
    ClientSecret: string
}

[<CLIMutable>]
type UsersSettings = {
    UseEntraMock: bool
}