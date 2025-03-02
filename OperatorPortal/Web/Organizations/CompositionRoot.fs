module Organizations.CompositionRoot

open Organizations.Application.ReadModels   

type Dependencies = {
    ReadOrganizationSummaries: ReadOrganizationSummaries
    ReadOrganizationDetailsBy: ReadOrganizationDetailsBy
}
