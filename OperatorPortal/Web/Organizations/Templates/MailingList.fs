module Organizations.Templates.MailingList

open Oxpecker.ViewEngine

type Params = {
    Mails: string
    MissingTeczka: string
}

let View (param: Params) =
    Fragment() {
        input(type'="text", value=param.Mails, id="email-buffer", hidden="true")
        input(type'="text", value=param.MissingTeczka, id="email-missing-teczka", hidden="true")
    }
