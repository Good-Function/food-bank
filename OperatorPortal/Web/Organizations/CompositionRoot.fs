module Organizations.CompositionRoot

open System.Data
open Organizations.Application
open Organizations.Application.ReadModels
open Organizations.Database

type Dependencies =
    { ReadOrganizationSummaries: ReadOrganizationSummaries
      ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
      ChangeDaneAdresowe: CommandHandlers.ChangeDaneAdresowe
      ChangeKontakty: CommandHandlers.ChangeKontakty
      ChangeBeneficjenci: CommandHandlers.ChangeBeneficjenci
      ChangeDokumenty: CommandHandlers.ChangeDokumenty
      ChangeAdresyKsiegowosci: CommandHandlers.ChangeAdresyKsiegowosci
      ChangeZrodlaZywnosci: CommandHandlers.ChangeZrodlaZywnosci
      ChangeWarunkiPomocy: CommandHandlers.ChangeWarunkiPomocy
      Import: CreateOrganizationCommandHandler.Import }

let build (connectDb: unit -> Async<IDbConnection>) : Dependencies =
    { ReadOrganizationSummaries = OrganizationsDao.readSummaries connectDb
      ReadOrganizationDetailsBy = OrganizationsDao.readBy connectDb
      ChangeDaneAdresowe = OrganizationsDao.changeDaneAdresowe connectDb
      ChangeKontakty = OrganizationsDao.changeKontakty connectDb
      ChangeBeneficjenci = OrganizationsDao.changeBeneficjenci connectDb
      ChangeDokumenty = OrganizationsDao.changeDokumenty connectDb
      ChangeAdresyKsiegowosci = OrganizationsDao.changeAdresyKsiegowosci connectDb
      ChangeZrodlaZywnosci = OrganizationsDao.changeZrodlaZywnosci connectDb
      ChangeWarunkiPomocy = OrganizationsDao.changeWarunkiPomocy connectDb
      Import = CreateOrganizationCommandHandler.importOrganizations
                ClosedXmlExcelImport.import
                (OrganizationsDao.saveMany connectDb)
       }
