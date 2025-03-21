module Organizations.Database.csvLoader

open System
open FSharp.Data
open Organizations.Application.ReadModels

[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__

type Orgs = CsvProvider<"db_sample.csv", ResolutionFolder=ResolutionFolder>

let polishToBool =
    function
    | "tak" -> true
    | _ -> false

let parseOptionalToDateOnly (str: string) =
    let success, date = DateOnly.TryParse str

    match success with
    | false -> None
    | _ -> Some date

let parse (org: Orgs.Row) : OrganizationDetails =
    { Teczka = org.Teczka
      IdentyfikatorEnova = org.``Identyfikator ENOVA``
      NIP = org.NIP
      Regon = org.Regon
      KrsNr = org.``Krs/ nr w rejestrze``
      FormaPrawna = org.``Forma prawna``
      OPP = org.OPP |> polishToBool
      DaneAdresowe =
        { NazwaOrganizacjiPodpisujacejUmowe = org.``Nazwa organizacji, która podpisała umowę``
          AdresRejestrowy = org.``Adres rejestrowy``
          NazwaPlacowkiTrafiaZywnosc = org.``Nazwa placówki do której trafia żywność``
          AdresPlacowkiTrafiaZywnosc = org.``Adres placówki, do której trafia żywność``
          GminaDzielnica = org.``Gmina/ Dzielnica``
          Powiat = org.Powiat }
      AdresyKsiegowosci = {
          NazwaOrganizacjiKsiegowanieDarowizn = org.``Nazwa organizacji na którą wystawiamy WZ (Księgowanie darowizn)``
          KsiegowanieAdres = org.``Księgowanie adres``
          TelOrganProwadzacegoKsiegowosc = org.``Tel. organ prowadzącego księgowość``
      }
      Kontakty =
        { WwwFacebook = org.``Www/ facebook``
          Telefon = org.Telefon
          Przedstawiciel = org.Przedstawiciel
          Kontakt = org.Kontakt
          Email = org.``E-mail``
          Dostepnosc = org.Dostępność
          OsobaDoKontaktu = org.``Osoba do kontaktu``
          TelefonOsobyKontaktowej = org.``Telefon do osoby kontaktowej``
          MailOsobyKontaktowej = org.``Mail osoby kontaktowej``
          OsobaOdbierajacaZywnosc = org.``Osoba odbierająca żywność``
          TelefonOsobyOdbierajacej = org.``Telefon do osoby kontaktowej`` }
      Beneficjenci =
        { LiczbaBeneficjentow = int org.``Liczba beneficjentów``
          Beneficjenci = org.Beneficjenci }
      ZrodlaZywnosci = {
          Sieci = org.Sieci |> polishToBool
          Bazarki = org.Bazarki |> polishToBool
          Machfit = org.Machfit |> polishToBool
          FEPZ2024 = org.``FEPŻ 2024`` |> polishToBool
      }
      WarunkiPomocy = {
          Kategoria = org.Kategoria
          RodzajPomocy = org.``RODZAJ POMOCY``
          SposobUdzielaniaPomocy = org.``Sposób udzielania pomocy``
          WarunkiMagazynowe = org.``Warunki magazynowe``
          HACCP = org.Haccp |> polishToBool
          Sanepid = org.SANEPID |> polishToBool
          TransportOpis = org.``Transport - opis``
          TransportKategoria = org.``Transport - kategoria``
      }
      Dokumenty =
        { Wniosek = org.Wniosek |> Option.map DateOnly.FromDateTime
          UmowaZDn = org.``Umowa z dnia`` |> Option.map DateOnly.FromDateTime
          UmowaRODO = org.``Umowa RODO`` |> parseOptionalToDateOnly
          KartyOrganizacjiData = org.``Karty organizacji data`` |> Option.map DateOnly.FromDateTime
          OstatnieOdwiedzinyData = org.``Ostatnie odwiedziny data`` |> Option.map DateOnly.FromDateTime } }
