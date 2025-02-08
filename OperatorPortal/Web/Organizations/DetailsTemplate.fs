module Organizations.DetailsTemplate

open System
open Organizations.Application.ReadModels
open Oxpecker.ViewEngine

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


let Template (org: OrganizationDetails) =
    div (class' = "grid") {
        div () {
            article () {
                header () { "Identyfikatory" }
                field "ENOVA" $"%i{org.IdentyfikatorEnova}"
                field "NIP" $"%i{org.NIP}"
                field "Regon" $"%i{org.Regon}"
                field "KRS" org.KrsNr
                field "Forma Prawna" org.FormaPrawna
                field "OPP" (org.OPP |> toTakNie)
            }
            
            article() {
                header () { "Kontakty" }
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
            
            article() {
                header () { "Dokumenty" }
                field "Wniosek" (org.Wniosek |> formatDate)
                field "Umowa z dnia" (org.UmowaZDn |> formatDate)
                field "Umowa z RODO" org.UmowaRODO
                field "Karty organizacji" (org.KartyOrganizacjiData |> formatDate)
                field "Ostatnie odwiedziny" (org.OstatnieOdwiedzinyData |> formatDate)
            }
        }

        div () {
            article () {
                header () { "Dane adresowe" }
                field "Organizacja, która podpisała umowę" org.NazwaOrganizacjiPodpisujacejUmowe
                field "Adres rejestrowy" org.AdresRejestrowy
                field "Placówka do której trafia żywność" org.NazwaPlacowkiTrafiaZywnosc
                field "Adres dostawy żywności" org.AdresPlacowkiTrafiaZywnosc
                field "Gmina / Dzielnica" org.GminaDzielnica
                field "Powiat" org.Powiat
            }
            
            article () {
                header () { "Dane adresowe księgowości" }
                field "Organizacja na którą wystawiamy WZ" org.NazwaOrganizacjiKsiegowanieDarowizn
                field "Adres" org.KsiegowanieAdres
                field "Telefon" org.TelOrganProwadzacegoKsiegowosc
            }
            
            article () {
                header () { "Beneficjenci" }
                field "Liczba beneficjentów" $"%i{org.LiczbaBeneficjentow}"
                field "Beneficjenci" org.Beneficjenci
            }
            
            article () {
                header () {"Żródła żywności"}
                field "Sieci" (org.Sieci |> toTakNie)
                field "Bazarki" (org.Bazarki |> toTakNie)
                field "Machfit" (org.Machfit |> toTakNie)
                field "FEPŻ 2024" (org.FEPZ2024 |> toTakNie)
            }
            
            article () {
                header () { "Warunki udzielania pomocy żywnościowej" }
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
