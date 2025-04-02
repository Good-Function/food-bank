module Organizations.Application.Commands

type DaneAdresowe =
    { Teczka: int64
      NazwaOrganizacjiPodpisujacejUmowe: string
      AdresRejestrowy: string
      NazwaPlacowkiTrafiaZywnosc: string
      AdresPlacowkiTrafiaZywnosc: string
      GminaDzielnica: string
      Powiat: string }

type Kontakty =
    { Teczka: int64
      WwwFacebook: string
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

type Dokumenty =
    { Teczka: int64
      Wniosek: System.DateOnly option
      UmowaZDn: System.DateOnly option
      UmowaRODO: System.DateOnly option
      KartyOrganizacjiData: System.DateOnly option
      OstatnieOdwiedzinyData: System.DateOnly option }

type Beneficjenci =
    { Teczka: int64
      LiczbaBeneficjentow: int
      Beneficjenci: string }

type ZrodlaZywnosci =
    { Teczka: int64
      Sieci: bool
      Bazarki: bool
      Machfit: bool
      FEPZ2024: bool
      OdbiorKrotkiTermin: bool
      TylkoNaszMagazyn: bool }

type AdresyKsiegowosci =
    { Teczka: int64
      NazwaOrganizacjiKsiegowanieDarowizn: string
      KsiegowanieAdres: string
      TelOrganProwadzacegoKsiegowosc: string }

type WarunkiPomocy =
    { Teczka: int64
      Kategoria: string
      RodzajPomocy: string
      SposobUdzielaniaPomocy: string
      WarunkiMagazynowe: string
      HACCP: bool
      Sanepid: bool
      TransportOpis: string
      TransportKategoria: string }

type ChangeZrodlaZywnosci = ZrodlaZywnosci -> Async<unit>
type ChangeAdresyKsiegowosci = AdresyKsiegowosci -> Async<unit>
type ChangeDaneAdresowe = DaneAdresowe -> Async<unit>
type ChangeKontakty = Kontakty -> Async<unit>
type ChangeBeneficjenci = Beneficjenci -> Async<unit>
type ChangeDokumenty = Dokumenty -> Async<unit>
type ChangeWarunkiPomocy = WarunkiPomocy -> Async<unit>
