module Organizations.DetailsTemplate

open System
open Layout
open Layout.Navigation
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine
open Web.Organizations
open renderOrganizationPage

let field (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        small () { value }
    }

let toTakNie (isTrue: bool) =
    $"""{if isTrue then "Tak" else "Nie"}"""

let formatDate (dateOpt: DateTime option) : string =
    match dateOpt with
    | Some date -> date.ToString("dd.MM.yyyy", System.Globalization.CultureInfo("pl-PL"))
    | None -> ""

let editableHeader (name: string) =
    header (class' = "action-header") {
        span () { name }
        span () { Icons.Pen }
    }

let Template (org: OrganizationDetails) =
    Fragment() {
        h3 () { $"Teczka {org.Teczka}, {org.NazwaPlacowkiTrafiaZywnosc}" }

        div (class' = "grid") {
            div () {
                article () {
                    editableHeader "Identyfikatory"
                    field "ENOVA" $"%i{org.IdentyfikatorEnova}"
                    field "NIP" $"%i{org.NIP}"
                    field "Regon" $"%i{org.Regon}"
                    field "KRS" org.KrsNr
                    field "Forma Prawna" org.FormaPrawna
                    field "OPP" (org.OPP |> toTakNie)
                }

                article () {
                    editableHeader "Kontakty"
                    field "www / facebook" org.WwwFacebook
                    field "Telefon" org.Telefon
                    field "Przedstawiciel" org.Przedstawiciel
                    field "Kontakt" org.Kontakt
                    field "E-mail" org.Email
                    field "Dostępność" org.Dostepnosc
                    field "Osoba do kontaktu" org.OsobaDoKontaktu
                    field "Telefon do os. kontaktowej" org.TelefonOsobyKontaktowej
                    field "E-mail do osoby kontaktowej" org.MailOsobyKontaktowej
                    field "Osoba odbierająca żywność" org.OsobaOdbierajacaZywnosc
                    field "Telefon do os. odbierającej" org.TelefonOsobyOdbierajacej
                }

                article () {
                    editableHeader "Dokumenty"
                    field "Wniosek" (org.Wniosek |> formatDate)
                    field "Umowa z dnia" (org.UmowaZDn |> formatDate)
                    field "Umowa z RODO" org.UmowaRODO
                    field "Karty organizacji" (org.KartyOrganizacjiData |> formatDate)
                    field "Ostatnie odwiedziny" (org.OstatnieOdwiedzinyData |> formatDate)
                }
            }

            div () {
                article () {
                    editableHeader "Dane adresowe"
                    field "Organizacja, która podpisała umowę" org.NazwaOrganizacjiPodpisujacejUmowe
                    field "Adres rejestrowy" org.AdresRejestrowy
                    field "Placówka do której trafia żywność" org.NazwaPlacowkiTrafiaZywnosc
                    field "Adres dostawy żywności" org.AdresPlacowkiTrafiaZywnosc
                    field "Gmina / Dzielnica" org.GminaDzielnica
                    field "Powiat" org.Powiat
                }

                article () {
                    editableHeader "Dane adresowe księgowości"
                    field "Organizacja na którą wystawiamy WZ" org.NazwaOrganizacjiKsiegowanieDarowizn
                    field "Adres" org.KsiegowanieAdres
                    field "Telefon" org.TelOrganProwadzacegoKsiegowosc
                }

                article () {
                    editableHeader "Beneficjenci"
                    field "Liczba beneficjentów" $"%i{org.LiczbaBeneficjentow}"
                    field "Beneficjenci" org.Beneficjenci
                }

                article () {
                    editableHeader "Żródła żywności"
                    field "Sieci" (org.Sieci |> toTakNie)
                    field "Bazarki" (org.Bazarki |> toTakNie)
                    field "Machfit" (org.Machfit |> toTakNie)
                    field "FEPŻ 2024" (org.FEPZ2024 |> toTakNie)
                }

                article () {
                    editableHeader "Warunki udzielania pomocy żywnościowej"
                    field "Kategoria" org.Kategoria
                    field "Rodzaj pomocy" org.RodzajPomocy
                    field "Sposób udzielania pomocy" org.SposobUdzielaniaPomocy
                    field "Warunki magazynowe" org.WarunkiMagazynowe
                    field "HACCP" (org.HACCP |> toTakNie)
                    field "Sanepid" (org.Sanepid |> toTakNie)
                    field "Transport - opis" org.TransportOpis
                    field "Transport - kategoria" org.TransportKategoria
                }
            }
        }
    }
    
let FullPage (org: OrganizationDetails) =
    composePage {Content = Template org; CurrentPage = Page.Organizations}
