module Organizations.Application.ReadModels

open System

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
      Kategoria: string
      OstatnieOdwiedzinyData: DateOnly option }

type DaneAdresowe =
    { NazwaOrganizacjiPodpisujacejUmowe: string
      AdresRejestrowy: string
      NazwaPlacowkiTrafiaZywnosc: string
      AdresPlacowkiTrafiaZywnosc: string
      GminaDzielnica: string
      Powiat: string }

    static member FromCommand(cmd: Commands.DaneAdresowe) =
        { NazwaOrganizacjiPodpisujacejUmowe = cmd.NazwaOrganizacjiPodpisujacejUmowe
          AdresRejestrowy = cmd.AdresRejestrowy
          NazwaPlacowkiTrafiaZywnosc = cmd.NazwaPlacowkiTrafiaZywnosc
          AdresPlacowkiTrafiaZywnosc = cmd.AdresPlacowkiTrafiaZywnosc
          GminaDzielnica = cmd.GminaDzielnica
          Powiat = cmd.Powiat }

type Beneficjenci =
    { LiczbaBeneficjentow: int
      Beneficjenci: string }

    static member FromCommand(cmd: Commands.Beneficjenci) =
        { LiczbaBeneficjentow = cmd.LiczbaBeneficjentow
          Beneficjenci = cmd.Beneficjenci }

type Dokumenty =
    { Wniosek: DateOnly option
      UmowaZDn: DateOnly option
      UmowaRODO: DateOnly option
      KartyOrganizacjiData: DateOnly option
      OstatnieOdwiedzinyData: DateOnly option
      DataUpowaznieniaDoOdbioru: DateOnly option }

    static member FromCommand(cmd: Commands.Dokumenty) =
        { Wniosek = cmd.Wniosek
          UmowaZDn = cmd.UmowaZDn
          UmowaRODO = cmd.UmowaRODO
          KartyOrganizacjiData = cmd.KartyOrganizacjiData
          OstatnieOdwiedzinyData = cmd.OstatnieOdwiedzinyData
          DataUpowaznieniaDoOdbioru = cmd.DataUpowaznieniaDoOdbioru }

type ZrodlaZywnosci =
    { Sieci: bool
      Bazarki: bool
      Machfit: bool
      FEPZ2024: bool // Fundusze europejskie na pomoc żywnościową, program UE.
      OdbiorKrotkiTermin: bool
      TylkoNaszMagazyn: bool }

    static member FromCommand(cmd: Commands.ZrodlaZywnosci) =
        { Sieci = cmd.Sieci
          Bazarki = cmd.Bazarki
          Machfit = cmd.Machfit
          FEPZ2024 = cmd.FEPZ2024
          OdbiorKrotkiTermin = cmd.OdbiorKrotkiTermin
          TylkoNaszMagazyn = cmd.TylkoNaszMagazyn }

type AdresyKsiegowosci =
    { NazwaOrganizacjiKsiegowanieDarowizn: string
      KsiegowanieAdres: string
      TelOrganProwadzacegoKsiegowosc: string }

    static member FromCommand(cmd: Commands.AdresyKsiegowosci) =
        { NazwaOrganizacjiKsiegowanieDarowizn = cmd.NazwaOrganizacjiKsiegowanieDarowizn
          KsiegowanieAdres = cmd.KsiegowanieAdres
          TelOrganProwadzacegoKsiegowosc = cmd.TelOrganProwadzacegoKsiegowosc }


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

    static member FromCommand(cmd: Commands.Kontakty) =
        { WwwFacebook = cmd.WwwFacebook
          Telefon = cmd.Telefon
          Przedstawiciel = cmd.Przedstawiciel
          Kontakt = cmd.Kontakt
          Email = cmd.Email
          Dostepnosc = cmd.Dostepnosc
          OsobaDoKontaktu = cmd.OsobaDoKontaktu
          TelefonOsobyKontaktowej = cmd.TelefonOsobyKontaktowej
          MailOsobyKontaktowej = cmd.MailOsobyKontaktowej
          OsobaOdbierajacaZywnosc = cmd.OsobaOdbierajacaZywnosc
          TelefonOsobyOdbierajacej = cmd.TelefonOsobyOdbierajacej }

type WarunkiPomocy =
    { Kategoria: string
      RodzajPomocy: string
      SposobUdzielaniaPomocy: string
      WarunkiMagazynowe: string
      HACCP: bool // Hard analysis and critical control point - system bezpieczenstwa zywnosci. Np podpisujesz sie ze sprawdziles temperatura w chlodni. Teoretycznie kazda org powinna to miec.
      Sanepid: bool // Zgoda sanepidu. Suche paczki mogą pójść bez sanepidu.
      TransportOpis: string
      TransportKategoria: string }

    static member FromCommand(cmd: Commands.WarunkiPomocy) =
        { Kategoria = cmd.Kategoria
          RodzajPomocy = cmd.RodzajPomocy
          SposobUdzielaniaPomocy = cmd.SposobUdzielaniaPomocy
          WarunkiMagazynowe = cmd.WarunkiMagazynowe
          HACCP = cmd.HACCP
          Sanepid = cmd.Sanepid
          TransportOpis = cmd.TransportOpis
          TransportKategoria = cmd.TransportKategoria }

type OrganizationDetails =
    { Teczka: int64
      IdentyfikatorEnova: string
      NIP: string
      Regon: string
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

type Direction =
    | Asc
    | Desc

    override this.ToString() =
        this
        |> function
            | Asc -> "asc"
            | Desc -> "desc"

    static member FromString(str: string) = if str = "desc" then Desc else Asc

type Filter =
    { searchTerm: string
      sortBy: (string * Direction) option}

type ReadOrganizationSummaries = Filter -> Async<OrganizationSummary list>
type ReadOrganizationDetailsBy = int64 -> Async<OrganizationDetails>
