module Organizations.Templates.DetailsTemplate

open System
open Layout.Navigation
open Organizations.Application.ReadModels.OrganizationDetails
open Layout.Fields
open Oxpecker.ViewEngine
open Organizations.Templates.PageComposer
open Organizations.Templates.Formatters

let Template (org: OrganizationDetails) permissions =
    Fragment() {
        h3 () { $"Teczka {org.Teczka}, {org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc}" }

        div (class' = "grid") {
            div () {
                article () {
                    header() { "Identyfikatory" }
                    readonlyField "ENOVA" $"{org.IdentyfikatorEnova}"
                    readonlyField "NIP" $"{org.NIP}"
                    readonlyField "Regon" $"{org.Regon}"
                    readonlyField "KRS" org.KrsNr
                    readonlyField "Forma Prawna" org.FormaPrawna
                    readonlyField "OPP" (org.OPP |> toTakNie)
                }
                
                Kontakty.View org.Kontakty org.Teczka permissions
                Dokumenty.View org.Dokumenty org.Teczka permissions
            }

            div () {
                DaneAdresowe.View org.DaneAdresowe org.Teczka permissions
                AdresyKsiegowosci.View org.AdresyKsiegowosci org.Teczka permissions
                Beneficjenci.View org.Beneficjenci org.Teczka permissions
                ZrodlaZywnosci.View org.ZrodlaZywnosci org.Teczka permissions
                WarunkiPomocy.View org.WarunkiPomocy org.Teczka permissions
            }
        }
    }
    
let FullPage (org: OrganizationDetails) permissions =
    composeFullPage {Content = Template org permissions; CurrentPage = Page.Organizations}
