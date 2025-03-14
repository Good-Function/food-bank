module Organizations.Application.ReadModels

type OrganizationSummary =
    { Teczka: int64
      FormaPrawna: string
      NazwaPlacowkiTrafiaZywnosc: string
      AdresPlacowkiTrafiaZywnosc: string
      GminaDzielnica: string
      Telefon: string
      Kontakt: string
      Email: string
      Dostepnosc: string
      OsobaDoKontaktu: string
      TelefonOsobyKontaktowej: string
      LiczbaBeneficjentow: int
      Kategoria: string }

type DaneAdresowe =
    { NazwaOrganizacjiPodpisujacejUmowe: string
      AdresRejestrowy: string
      NazwaPlacowkiTrafiaZywnosc: string
      AdresPlacowkiTrafiaZywnosc: string
      GminaDzielnica: string
      Powiat: string }
    
type Beneficjenci = {
      LiczbaBeneficjentow: int
      Beneficjenci: string
}

type Dokumenty = {
      Wniosek: System.DateOnly option
      UmowaZDn: System.DateOnly option
      UmowaRODO: System.DateOnly option
      KartyOrganizacjiData: System.DateOnly option
      OstatnieOdwiedzinyData: System.DateOnly option
}

type ZrodlaZywnosci = {
      Sieci: bool
      Bazarki: bool
      Machfit: bool
      FEPZ2024: bool // Fundusze europejskie na pomoc żywnościową, program UE.
}

type AdresyKsiegowosci = {
      NazwaOrganizacjiKsiegowanieDarowizn: string
      KsiegowanieAdres: string
      TelOrganProwadzacegoKsiegowosc: string
}

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

type OrganizationDetails =
    { Teczka: int64
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
      Kategoria: string
      RodzajPomocy: string
      SposobUdzielaniaPomocy: string
      WarunkiMagazynowe: string
      HACCP: bool // Hard analysis and critical control point - system bezpieczenstwa zywnosci. Np podpisujesz sie ze sprawdziles temperatura w chlodni. Teoretycznie kazda org powinna to miec.
      Sanepid: bool // Zgoda sanepidu. Suche paczki mogą pójść bez sanepidu.
      TransportOpis: string
      TransportKategoria: string
      Dokumenty: Dokumenty }

type ReadOrganizationSummaries = string -> Async<OrganizationSummary list>
type ReadOrganizationDetailsBy = int64 -> Async<OrganizationDetails>
