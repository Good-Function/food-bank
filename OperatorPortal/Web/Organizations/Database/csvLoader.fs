module Organizations.Database.csvLoader

open FSharp.Data
open OrganizationRow

[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__

type Orgs = CsvProvider<"db_sample.csv", ResolutionFolder=ResolutionFolder>

let polishToBool = function | "tak" -> true |_ -> false

let parse (org: Orgs.Row): OrganizationRow=
    { Teczka = org.Teczka
      IdentyfikatorEnova = org.``Identyfikator ENOVA``
      NIP = org.NIP
      Regon = org.Regon
      KrsNr = org.``Krs/ nr w rejestrze``
      FormaPrawna = org.``Forma prawna``
      OPP = org.OPP |> polishToBool
      NazwaOrganizacjiPodpisujacejUmowe = org.``Nazwa organizacji, która podpisała umowę``
      AdresRejestrowy = org.``Adres rejestrowy``
      NazwaPlacowkiTrafiaZywnosc = org.``Nazwa placówki do której trafia żywność``
      AdresPlacowkiTrafiaZywnosc = org.``Adres placówki, do której trafia żywność``
      GminaDzielnica = org.``Gmina/ Dzielnica``
      Powiat = org.Powiat
      NazwaOrganizacjiKsiegowanieDarowizn = org.``Nazwa organizacji na którą wystawiamy WZ (Księgowanie darowizn)``
      KsiegowanieAdres = org.``Księgowanie adres``
      TelOrganProwadzacegoKsiegowosc = org.``Tel. organ prowadzącego księgowość``
      WwwFacebook = org.``Www/ facebook``
      Telefon = org.Telefon
      Przedstawiciel = org.Przedstawiciel
      Kontakt = org.Kontakt
      Fax = org.Fax
      Email = org.``E-mail``
      Dostepnosc = org.Dostępność
      OsobaDoKontaktu = org.``Osoba do kontaktu``
      TelefonOsobyKontaktowej = org.``Telefon do osoby kontaktowej``
      MailOsobyKontaktowej = org.``Mail osoby kontaktowej``
      OsobaOdbierajacaZywnosc = org.``Osoba odbierająca żywność``
      TelefonOsobyOdbierajacej = org.``Telefon do osoby kontaktowej``
      LiczbaBeneficjentow = int org.``Liczba beneficjentów``
      Beneficjenci = org.Beneficjenci
      Sieci = org.Sieci |> polishToBool
      Bazarki = org.Bazarki |> polishToBool
      Machfit = org.Machfit |> polishToBool
      FEPZ2024 = org.``FEPŻ 2024`` |> polishToBool
      Kategoria = org.Kategoria
      RodzajPomocy = org.``RODZAJ POMOCY``
      SposobUdzielaniaPomocy = org.``Sposób udzielania pomocy``
      WarunkiMagazynowe = org.``Warunki magazynowe``
      HACCP = org.Haccp |> polishToBool
      Sanepid = org.SANEPID |> polishToBool
      TransportOpis = org.``Transport - opis``
      TransportKategoria = org.``Transport - kategoria``
      Wniosek = org.Wniosek
      UmowaZDn = org.``Umowa z dnia``
      UmowaRODO = org.``Umowa RODO``
      KartyOrganizacjiData = org.``Karty organizacji data``
      OstatnieOdwiedzinyData = org.``Ostatnie odwiedziny data``
    }