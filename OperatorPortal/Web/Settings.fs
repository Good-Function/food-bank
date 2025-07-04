module Settings

[<CLIMutable>]
type AzureAdSettings = {
    Instance: string
    Domain: string
    TenantId: string
    ClientId: string
    CallbackPath: string
    ClientSecret: string //AzureAd__ClientSecret
}


[<CLIMutable>]
type Settings = {
      DbConnectionString: string
      BlobStorageConnectionString: string
      AzureAd: AzureAdSettings
    }
