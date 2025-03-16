module Organizations.Templates.DetailsTemplate

open System
open Layout
open Layout.Navigation
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Web.Organizations
open PageComposer
open Web.Organizations.Templates.Formatters

let field (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        small () { value }
    }

let editableHeader (name: string) =
    header (class' = "action-header") {
        span () { name }
        div(class' = "action-header-actions") {
            span () { Icons.Pen }
        }
    }

let Template (org: OrganizationDetails) =
    Fragment() {
        h3 () { $"Teczka {org.Teczka}, {org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc}" }

        div (class' = "grid") {
            div () {
                article () {
                    header() { "Identyfikatory" }
                    field "ENOVA" $"{org.IdentyfikatorEnova}"
                    field "NIP" $"{org.NIP}"
                    field "Regon" $"{org.Regon}"
                    field "KRS" org.KrsNr
                    field "Forma Prawna" org.FormaPrawna
                    field "OPP" (org.OPP |> toTakNie)
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
