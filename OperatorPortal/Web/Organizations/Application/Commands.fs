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

type ChangeDaneAdresowe = DaneAdresowe -> Async<unit>
type ChangeKontakty = Kontakty -> Async<unit>
type ChangeBeneficjenci = Beneficjenci -> Async<unit>
type ChangeDokumenty = Dokumenty -> Async<unit>
