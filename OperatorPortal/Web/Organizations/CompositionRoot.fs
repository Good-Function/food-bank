module Organizations.CompositionRoot

open System.Data
open Azure.Storage.Blobs
open Organizations.Application
open Organizations.Application.ReadModels
open Organizations.Application.ReadModels.ReadAuditTrail
open Organizations.Application.ReadModels.OrganizationDetails
open Organizations.Application.ReadModels.OrganizationSummary
open Organizations.Database

type Dependencies =
    { ReadOrganizationSummaries: ReadOrganizationSummaries
      ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
      ReadAuditTrail: ReadAuditTrail
      ReadMailingList: MailingList.ReadMailingList
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
      ReadMailingList = OrganizationsDao.readMailingListBy connectDb
      ReadOrganizationDetailsBy = OrganizationsDao.readDetailsBy connectDb
      ChangeDaneAdresowe =
        Handlers.changeDaneAdresowe
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeKontakty =
        Handlers.changeKontakty
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeBeneficjenci =
        Handlers.changeBeneficjenci
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeAdresyKsiegowosci =
        Handlers.changeAdresyKsiegowosci
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeZrodlaZywnosci =
        Handlers.changeZrodlaZywnosci
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeWarunkiPomocy =
        Handlers.changeWarunkiPomocy
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      SaveDocument =
        DocumentHandlers.saveDocumentHandler
            (BlobStorage.upload blobServiceClient)
            (OrganizationsDao.saveDocMetadata connectDb)
      DeleteDocument = BlobStorage.delete blobServiceClient
      ReadAuditTrail = AuditTrailDao.AuditTrailDao(connectDb).ReadAuditTrail
      GenerateDownloadUri = BlobStorage.generateDownloadUri blobServiceClient
      Import =
        CreateOrganizationCommandHandler.importOrganizations
            ClosedXmlExcelImport.import
            (OrganizationsDao.saveMany connectDb) }

type DataApiDependencies =
    { ReadOrganizationDetailsByEmail: ReadOrganizationDetailsByEmail
      ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
      ChangeDaneAdresowe: Handlers.ChangeDaneAdresowe
      ChangeKontakty: Handlers.ChangeKontakty
      ChangeBeneficjenci: Handlers.ChangeBeneficjenci
      ChangeAdresyKsiegowosci: Handlers.ChangeAdresyKsiegowosci
      ChangeZrodlaZywnosci: Handlers.ChangeZrodlaZywnosci
      ChangeWarunkiPomocy: Handlers.ChangeWarunkiPomocy }

let buildDataApi (connectDb: unit -> Async<IDbConnection>) : DataApiDependencies =
    { ReadOrganizationDetailsByEmail = OrganizationsDao.readByEmail connectDb
      ReadOrganizationDetailsBy = OrganizationsDao.readDetailsBy connectDb
      ChangeDaneAdresowe =
        Handlers.changeDaneAdresowe
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeKontakty =
        Handlers.changeKontakty
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeBeneficjenci =
        Handlers.changeBeneficjenci
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeAdresyKsiegowosci =
        Handlers.changeAdresyKsiegowosci
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeZrodlaZywnosci =
        Handlers.changeZrodlaZywnosci
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)
      ChangeWarunkiPomocy =
        Handlers.changeWarunkiPomocy
            (OrganizationsDao.readBy connectDb)
            (OrganizationsDao.save connectDb)
            (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)

    }
