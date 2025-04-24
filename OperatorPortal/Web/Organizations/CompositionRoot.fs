module Organizations.CompositionRoot

open System.Data
open Organizations.Application
open Organizations.Application.ReadModels
open Organizations.Database

type Dependencies =
    { ReadOrganizationSummaries: ReadOrganizationSummaries
      ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
      ChangeDaneAdresowe: ChangeOrgazniationCommands.ChangeDaneAdresowe
      ChangeKontakty: ChangeOrgazniationCommands.ChangeKontakty
      ChangeBeneficjenci: ChangeOrgazniationCommands.ChangeBeneficjenci
      ChangeDokumenty: ChangeOrgazniationCommands.ChangeDokumenty
      ChangeAdresyKsiegowosci: ChangeOrgazniationCommands.ChangeAdresyKsiegowosci
      ChangeZrodlaZywnosci: ChangeOrgazniationCommands.ChangeZrodlaZywnosci
      ChangeWarunkiPomocy: ChangeOrgazniationCommands.ChangeWarunkiPomocy
      Import: CreateOrganizationCommands.Import }

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
      Import = CreateOrganizationCommands.importOrganizations
                ClosedXmlExcelImport.import
                (OrganizationsDao.saveMany connectDb)
       }
