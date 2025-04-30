module Organizations.Application.Commands

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
    { Wniosek: System.DateOnly option
      UmowaZDn: System.DateOnly option
      UmowaRODO: System.DateOnly option
      KartyOrganizacjiData: System.DateOnly option
      OstatnieOdwiedzinyData: System.DateOnly option
      DataUpowaznieniaDoOdbioru: System.DateOnly option }

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
