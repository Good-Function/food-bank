module Organizations.BlobStorage

open System
open Azure.Storage.Sas
open Azure.Storage.Blobs
open Organizations.Application
open Organizations.Application.DocumentHandlers

let upload (serviceClient: BlobServiceClient): UploadBlob =
    fun (teczkaId, uploadDocument) ->
        async {
            let containerClient = serviceClient.GetBlobContainerClient $"{teczkaId}"
            let! _ = containerClient.CreateIfNotExistsAsync() |> Async.AwaitTask
            let blobClient = containerClient.GetBlobClient uploadDocument.FileName
            let! _ = blobClient.UploadAsync(uploadDocument.ContentStream, true) |> Async.AwaitTask
            return ()
        }
    
let delete (serviceClient: BlobServiceClient) : DeleteFile =
    fun (teczkaId: Commands.TeczkaId, fileName: FileName) -> 
        async {
             let containerClient = serviceClient.GetBlobContainerClient($"{teczkaId}")
             let! _ = containerClient.DeleteBlobIfExistsAsync(fileName) |> Async.AwaitTask
             return ()
        }
    
let generateDownloadUri (serviceClient: BlobServiceClient) : GenerateDownloadUri =
    fun (teczkaId: Commands.TeczkaId, fileName:string) ->
        let containerClient = serviceClient.GetBlobContainerClient($"{teczkaId}")
        let blobClient = containerClient.GetBlobClient(fileName)
        let sasBuilder = BlobSasBuilder(
            BlobContainerName = $"{teczkaId}",
            BlobName = fileName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(3.0)
        )
        sasBuilder.SetPermissions(BlobSasPermissions.Read)
        blobClient.GenerateSasUri(sasBuilder)