module Settings


[<CLIMutable>]
type Settings =
    { DbConnectionString: string
      BlobStorageConnectionString: string }
