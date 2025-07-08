module Settings

open Users.Settings

[<CLIMutable>]
type Settings = {
      DbConnectionString: string
      BlobStorageConnectionString: string
      AzureAd: AzureAdSettings
      Users: UsersSettings
    }
