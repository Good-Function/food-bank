module Organizations.Application.ReadModels.MailingList

open Organizations.Application.ReadModels.Filter

type Contact = {Teczka: int64; Email: string}

type ReadMailingList = string * Filter list -> Async<Contact list>
