module Learning.BlobUpload

open System
open System.IO
open Azure.Storage.Sas
open Azure.Storage.Blobs
open FSharp.Data
open FsUnit.Xunit
open Xunit

let connectionString = "UseDevelopmentStorage=true"
let containerName = "organization-files"
let fileName = "file.txt"

[<Fact>]
let ``Upload blob to azurite`` () =
     task {
        use fs = File.OpenRead(Path.Combine(__SOURCE_DIRECTORY__, fileName))
        let blobServiceClient = BlobServiceClient(connectionString)
        let containerClient = blobServiceClient.GetBlobContainerClient(containerName)
        let! _ = containerClient.CreateIfNotExistsAsync()
        let blobClient = containerClient.GetBlobClient fileName
        let! _ = blobClient.UploadAsync(fs, true)
        // Generate SAS
        let sasBuilder = BlobSasBuilder(
                BlobContainerName = containerName,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5.0)
            )
        sasBuilder.SetPermissions(BlobSasPermissions.Read)
        let sasUri = blobClient.GenerateSasUri(sasBuilder)
        let uploadedFileContent = Http.RequestString (sasUri.ToString())
        uploadedFileContent |> should equal (File.ReadAllText(Path.Combine(__SOURCE_DIRECTORY__, fileName)))
     }
