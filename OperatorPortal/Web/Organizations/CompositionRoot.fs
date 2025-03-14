module Organizations.CompositionRoot

open Organizations.Application
open Organizations.Application.ReadModels   

type Dependencies = {
    ReadOrganizationSummaries: ReadOrganizationSummaries
    ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
    ChangeDaneAdresowe: Commands.ChangeDaneAdresowe
    ChangeKontakty: Commands.ChangeKontakty
    ChangeBeneficjenci: Commands.ChangeBeneficjenci
    ChangeDokumenty: Commands.ChangeDokumenty
    ChangeAdresyKsiegowosci: Commands.ChangeAdresyKsiegowosci
    ChangeZrodlaZywnosci: Commands.ChangeZrodlaZywnosci
}
