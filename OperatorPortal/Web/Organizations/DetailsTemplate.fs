module Organizations.DetailsTemplate

open Organizations.Application.ReadModels
open Oxpecker.ViewEngine

let Template (org: OrganizationDetails) =
    div () {
        article () {
            header () { "Identyfikatory" }
            label () { "ENOVA" }
            p () { $"{org.IdentyfikatorEnova}" }
            label () { "NIP" }
            p () { $"{org.NIP}" }
        }
    }
