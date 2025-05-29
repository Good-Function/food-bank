module Organizations.CompositionRoot

open System.Data
open Azure.Storage.Blobs
open Organizations.Application
open Organizations.Application.ReadModels
open Organizations.Database

type Dependencies =
    { ReadOrganizationSummaries: ReadOrganizationSummaries
      ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
      ChangeDaneAdresowe: Handlers.ChangeDaneAdresowe
      ChangeKontakty: Handlers.ChangeKontakty
      ChangeBeneficjenci: Handlers.ChangeBeneficjenci
      SaveDocument: DocumentHandlers.SaveFile
      DeleteDocument: DocumentHandlers.DeleteFile
      GenerateDownloadUri: DocumentHandlers.GenerateDownloadUri
      ChangeAdresyKsiegowosci: Handlers.ChangeAdresyKsiegowosci
      ChangeZrodlaZywnosci: Handlers.ChangeZrodlaZywnosci
      ChangeWarunkiPomocy: Handlers.ChangeWarunkiPomocy
      Import: CreateOrganizationCommandHandler.Import }

let build (connectDb: unit -> Async<IDbConnection>, blobServiceClient: BlobServiceClient) : Dependencies =
    { ReadOrganizationSummaries = OrganizationsDao.readSummaries connectDb
      ReadOrganizationDetailsBy = OrganizationsDao.readBy connectDb
      ChangeDaneAdresowe = OrganizationsDao.changeDaneAdresowe connectDb
      ChangeKontakty = OrganizationsDao.changeKontakty connectDb
      ChangeBeneficjenci = OrganizationsDao.changeBeneficjenci connectDb
      SaveDocument = DocumentHandlers.saveDocumentHandler
                        (BlobStorage.upload blobServiceClient)
                        (OrganizationsDao.saveDocMetadata connectDb)
      DeleteDocument = BlobStorage.delete blobServiceClient
      GenerateDownloadUri = BlobStorage.generateDownloadUri blobServiceClient
      ChangeAdresyKsiegowosci = OrganizationsDao.changeAdresyKsiegowosci connectDb
      ChangeZrodlaZywnosci = OrganizationsDao.changeZrodlaZywnosci connectDb
      ChangeWarunkiPomocy = OrganizationsDao.changeWarunkiPomocy connectDb
      Import = CreateOrganizationCommandHandler.importOrganizations
                ClosedXmlExcelImport.import
                (OrganizationsDao.saveMany connectDb)
       }
