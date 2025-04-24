module Organizations.Dtos

open Organizations.Application

[<CLIMutable>]
type DaneAdresoweForm =
    { NazwaOrganizacjiPodpisujacejUmowe: string
      AdresRejestrowy: string
      NazwaPlacowkiTrafiaZywnosc: string
      AdresPlacowkiTrafiaZywnosc: string
      GminaDzielnica: string
      Powiat: string }

    member this.toChangeDaneAdresowe: WriteModels.DaneAdresowe =
        { NazwaOrganizacjiPodpisujacejUmowe = this.NazwaOrganizacjiPodpisujacejUmowe
          AdresRejestrowy = this.AdresRejestrowy
          NazwaPlacowkiTrafiaZywnosc = this.NazwaPlacowkiTrafiaZywnosc
          AdresPlacowkiTrafiaZywnosc = this.AdresPlacowkiTrafiaZywnosc
          GminaDzielnica = this.GminaDzielnica
          Powiat = this.Powiat }

    member this.toDaneAdresowe: ReadModels.DaneAdresowe =
        { NazwaOrganizacjiPodpisujacejUmowe = this.NazwaOrganizacjiPodpisujacejUmowe
          AdresRejestrowy = this.AdresRejestrowy
          NazwaPlacowkiTrafiaZywnosc = this.NazwaPlacowkiTrafiaZywnosc
          AdresPlacowkiTrafiaZywnosc = this.AdresPlacowkiTrafiaZywnosc
          GminaDzielnica = this.GminaDzielnica
          Powiat = this.Powiat }

[<CLIMutable>]
type KontaktyForm =
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

    member this.toChangeKontakty: WriteModels.Kontakty =
        { WwwFacebook = this.WwwFacebook
          Telefon = this.Telefon
          Przedstawiciel = this.Przedstawiciel
          Kontakt = this.Kontakt
          Email = this.Email
          Dostepnosc = this.Dostepnosc
          OsobaDoKontaktu = this.OsobaDoKontaktu
          TelefonOsobyKontaktowej = this.TelefonOsobyKontaktowej
          MailOsobyKontaktowej = this.MailOsobyKontaktowej
          OsobaOdbierajacaZywnosc = this.OsobaOdbierajacaZywnosc
          TelefonOsobyOdbierajacej = this.TelefonOsobyOdbierajacej }

    member this.toKontakty: ReadModels.Kontakty =
        { WwwFacebook = this.WwwFacebook
          Telefon = this.Telefon
          Przedstawiciel = this.Przedstawiciel
          Kontakt = this.Kontakt
          Email = this.Email
          Dostepnosc = this.Dostepnosc
          OsobaDoKontaktu = this.OsobaDoKontaktu
          TelefonOsobyKontaktowej = this.TelefonOsobyKontaktowej
          MailOsobyKontaktowej = this.MailOsobyKontaktowej
          OsobaOdbierajacaZywnosc = this.OsobaOdbierajacaZywnosc
          TelefonOsobyOdbierajacej = this.TelefonOsobyOdbierajacej }

[<CLIMutable>]
type BeneficjenciForm =
    { LiczbaBeneficjentow: int
      Beneficjenci: string }

    member this.toChangeBeneficjenci: WriteModels.Beneficjenci =
        { LiczbaBeneficjentow = this.LiczbaBeneficjentow
          Beneficjenci = this.Beneficjenci }

    member this.toBeneficjenci: ReadModels.Beneficjenci =
        { LiczbaBeneficjentow = this.LiczbaBeneficjentow
          Beneficjenci = this.Beneficjenci }

[<CLIMutable>]
type DokumentyForm =
    { Wniosek: System.DateOnly option
      UmowaZDn: System.DateOnly option
      UmowaRODO: System.DateOnly option
      KartyOrganizacjiData: System.DateOnly option
      OstatnieOdwiedzinyData: System.DateOnly option }

    member this.toChangeDokumenty: WriteModels.Dokumenty =
        { Wniosek = this.Wniosek
          UmowaZDn = this.UmowaZDn
          UmowaRODO = this.UmowaRODO
          KartyOrganizacjiData = this.KartyOrganizacjiData
          OstatnieOdwiedzinyData = this.OstatnieOdwiedzinyData }

    member this.toDokumenty: ReadModels.Dokumenty =
        { Wniosek = this.Wniosek
          UmowaZDn = this.UmowaZDn
          UmowaRODO = this.UmowaRODO
          KartyOrganizacjiData = this.KartyOrganizacjiData
          OstatnieOdwiedzinyData = this.OstatnieOdwiedzinyData }

[<CLIMutable>]
type ZrodlaZywnosciForm =
    { Sieci: bool
      Bazarki: bool
      Machfit: bool
      FEPZ2024: bool
      OdbiorKrotkiTermin: bool
      TylkoNaszMagazyn: bool }

    member this.toChangeZrodlaZywnosci: WriteModels.ZrodlaZywnosci =
        { Sieci = this.Sieci
          Bazarki = this.Bazarki
          Machfit = this.Machfit
          FEPZ2024 = this.FEPZ2024
          OdbiorKrotkiTermin = this.OdbiorKrotkiTermin
          TylkoNaszMagazyn = this.TylkoNaszMagazyn }

    member this.toZrodlaZywnosci: ReadModels.ZrodlaZywnosci =
        { Sieci = this.Sieci
          Bazarki = this.Bazarki
          Machfit = this.Machfit
          FEPZ2024 = this.FEPZ2024
          OdbiorKrotkiTermin = this.OdbiorKrotkiTermin
          TylkoNaszMagazyn = this.TylkoNaszMagazyn }

[<CLIMutable>]
type AdresyKsiegowosciForm =
    { NazwaOrganizacjiKsiegowanieDarowizn: string
      KsiegowanieAdres: string
      TelOrganProwadzacegoKsiegowosc: string }

    member this.toChangeAdresyKsiegowosci : WriteModels.AdresyKsiegowosci =
        { NazwaOrganizacjiKsiegowanieDarowizn = this.NazwaOrganizacjiKsiegowanieDarowizn
          KsiegowanieAdres = this.KsiegowanieAdres
          TelOrganProwadzacegoKsiegowosc = this.TelOrganProwadzacegoKsiegowosc }

    member this.toAdresyKsiegowosci: ReadModels.AdresyKsiegowosci =
        { NazwaOrganizacjiKsiegowanieDarowizn = this.NazwaOrganizacjiKsiegowanieDarowizn
          KsiegowanieAdres = this.KsiegowanieAdres
          TelOrganProwadzacegoKsiegowosc = this.TelOrganProwadzacegoKsiegowosc }

[<CLIMutable>]
type WarunkiPomocyForm =
    { Kategoria: string
      RodzajPomocy: string
      SposobUdzielaniaPomocy: string
      WarunkiMagazynowe: string
      HACCP: bool
      Sanepid: bool
      TransportOpis: string
      TransportKategoria: string }

    member this.toChangeWarunkiPomocy: WriteModels.WarunkiPomocy =
        { Kategoria = this.Kategoria
          RodzajPomocy = this.RodzajPomocy
          SposobUdzielaniaPomocy = this.SposobUdzielaniaPomocy
          WarunkiMagazynowe = this.WarunkiMagazynowe
          HACCP = this.HACCP
          Sanepid = this.Sanepid
          TransportOpis = this.TransportOpis
          TransportKategoria = this.TransportKategoria }

    member this.toWarunkiPomocy: ReadModels.WarunkiPomocy =
        { Kategoria = this.Kategoria
          RodzajPomocy = this.RodzajPomocy
          SposobUdzielaniaPomocy = this.SposobUdzielaniaPomocy
          WarunkiMagazynowe = this.WarunkiMagazynowe
          HACCP = this.HACCP
          Sanepid = this.Sanepid
          TransportOpis = this.TransportOpis
          TransportKategoria = this.TransportKategoria }
