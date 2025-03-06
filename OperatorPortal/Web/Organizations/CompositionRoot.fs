module Organizations.CompositionRoot

open Organizations.Application
open Organizations.Application.ReadModels   

type Dependencies = {
    ReadOrganizationSummaries: ReadOrganizationSummaries
    ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
    ModifyDaneAdresowe: Commands.ChangeDaneAdresowe
}
