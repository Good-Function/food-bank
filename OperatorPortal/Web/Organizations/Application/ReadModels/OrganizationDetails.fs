module Organizations.Application.ReadModels.OrganizationDetails

open System
open Organizations.Application
open Organizations.Application.DocumentType

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

type Document = {
    Date: DateOnly option
    FileName: string option
    Type: DocumentType
}

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
      Dokumenty: Document list
      WarunkiPomocy: WarunkiPomocy }
    
type ReadOrganizationDetailsBy = int64 -> Async<OrganizationDetails>