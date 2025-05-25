module Organizations.Application.Commands

open System.IO

type TeczkaId = int64

[<CLIMutable>]
type DaneAdresowe =
    { NazwaOrganizacjiPodpisujacejUmowe: string
      AdresRejestrowy: string
      NazwaPlacowkiTrafiaZywnosc: string
      AdresPlacowkiTrafiaZywnosc: string
      GminaDzielnica: string
      Powiat: string }

[<CLIMutable>]
type Kontakty =
    { WwwFacebook: string
      Telefon: string
      Przedstawiciel: string
      Kontakt: string
      Email: string
      Dostepnosc: string
      OsobaDoKontaktu: string
      TelefonOsobyKontaktowej: string
      MailOsobyKontaktowej: string
      OsobaOdbierajacaZywnosc: string
      TelefonOsobyOdbierajacej: string }

[<CLIMutable>]
type Dokumenty =
    { Wniosek: string option
      WniosekDate: System.DateOnly option
      DeleteWniosek: string option
      Umowa: string option
      UmowaDate: System.DateOnly option
      DeleteUmowa: string option
      RODO: string option
      RODODate: System.DateOnly option
      DeleteRODO: string option
      Odwiedziny: string option
      OdwiedzinyDate: System.DateOnly option
      DeleteOdwiedziny: string option
      UpowaznienieDoOdbioru: string option
      UpowaznienieDoOdbioruDate: System.DateOnly option
      DeleteUpowaznienieDoOdbioru: string option }

[<CLIMutable>]
type Beneficjenci =
    { LiczbaBeneficjentow: int
      Beneficjenci: string }

[<CLIMutable>]
type ZrodlaZywnosci =
    { Sieci: bool
      Bazarki: bool
      Machfit: bool
      FEPZ2024: bool
      OdbiorKrotkiTermin: bool
      TylkoNaszMagazyn: bool }

[<CLIMutable>]
type AdresyKsiegowosci =
    { NazwaOrganizacjiKsiegowanieDarowizn: string
      KsiegowanieAdres: string
      TelOrganProwadzacegoKsiegowosc: string }

[<CLIMutable>]
type WarunkiPomocy =
    { Kategoria: string
      RodzajPomocy: string
      SposobUdzielaniaPomocy: string
      WarunkiMagazynowe: string
      HACCP: bool
      Sanepid: bool
      TransportOpis: string
      TransportKategoria: string }

type NewOrganization =
    { Teczka: TeczkaId
      IdentyfikatorEnova: int64
      NIP: int64
      Regon: int64
      KrsNr: string
      FormaPrawna: string // fundacja, stowarzyszenie, org koscielna
      OPP: bool // szczegóły: Organizacja użytku publicznego
      DaneAdresowe: DaneAdresowe
      Kontakty: Kontakty
      ZrodlaZywnosci: ZrodlaZywnosci
      AdresyKsiegowosci: AdresyKsiegowosci
      Beneficjenci: Beneficjenci
      Dokumenty: Dokumenty
      WarunkiPomocy: WarunkiPomocy }
