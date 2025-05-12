module Organizations.BlobStorage

open Azure.Storage.Blobs
open Organizations.Application

let upload (blobClient: BlobServiceClient) (teczkaId: Commands.TeczkaId, command: Commands.DocumentUpload) =
    async {
        let containerClient = blobClient.GetBlobContainerClient $"{teczkaId}"
        let! _ = containerClient.CreateIfNotExistsAsync() |> Async.AwaitTask
        let blobClient = containerClient.GetBlobClient command.Name
        let! _ = blobClient.UploadAsync(command.ContentStream, true) |> Async.AwaitTask
        return ()
    }