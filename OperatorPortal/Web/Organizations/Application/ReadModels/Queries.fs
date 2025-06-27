module Organizations.Application.ReadModels.Queries

open Organizations.Application.ReadModels.Filter

type ReadMailingList = string * Filter list -> Async<string list>
