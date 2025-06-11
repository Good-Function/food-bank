module Organizations.Templates.DetailsTemplate

open System
open Layout.Navigation
open Organizations.Application.ReadModels.OrganizationDetails
open Layout.Fields
open Oxpecker.ViewEngine
open Web.Organizations
open PageComposer
open Organizations.Templates.Formatters

let Template (org: OrganizationDetails) =
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
                
                Kontakty.View org.Kontakty org.Teczka
                Dokumenty.View org.Dokumenty org.Teczka
            }

            div () {
                DaneAdresowe.View org.DaneAdresowe org.Teczka
                AdresyKsiegowosci.View org.AdresyKsiegowosci org.Teczka
                Beneficjenci.View org.Beneficjenci org.Teczka
                ZrodlaZywnosci.View org.ZrodlaZywnosci org.Teczka
                WarunkiPomocy.View org.WarunkiPomocy org.Teczka
            }
        }
    }
    
let FullPage (org: OrganizationDetails) =
    composeFullPage {Content = Template org; CurrentPage = Page.Organizations}
