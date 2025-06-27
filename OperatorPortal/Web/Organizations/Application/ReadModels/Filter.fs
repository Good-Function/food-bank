module Organizations.Application.ReadModels.Filter

open Organizations.Application.ReadModels.FilterOperators
open Organizations.Application.ReadModels.QueriedColumn

type Filter = { Key: QueriedColumn; Value: obj; Operator: FilterOperator }