[<AutoOpen>]
module Transaction

let openTransaction() =
    new System.Transactions.TransactionScope(
        System.Transactions.TransactionScopeOption.Required,
        System.Transactions.TransactionScopeAsyncFlowOption.Enabled
    )
