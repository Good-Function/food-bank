module Organizations.CompositionRoot

open Organizations.Application
open Organizations.Application.ReadModels   

type Dependencies = {
    ReadOrganizationSummaries: ReadOrganizationSummaries
    ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
    ChangeDaneAdresowe: Commands.ChangeDaneAdresowe
    ChangeKontakty: Commands.ChangeKontakty
}
