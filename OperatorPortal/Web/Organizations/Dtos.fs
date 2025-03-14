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

    member this.toChangeDaneAdresowe(id: int64) : Commands.DaneAdresowe =
        { Teczka = id
          NazwaOrganizacjiPodpisujacejUmowe = this.NazwaOrganizacjiPodpisujacejUmowe
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

    member this.toChangeKontakty(id: int64) : Commands.Kontakty =
        { Teczka = id
          WwwFacebook = this.WwwFacebook
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

    member this.toChangeBeneficjenci(id: int64) : Commands.Beneficjenci =
        { Teczka = id
          LiczbaBeneficjentow = this.LiczbaBeneficjentow
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

    member this.toChangeDokumenty(id: int64) : Commands.Dokumenty =
        { Teczka = id
          Wniosek = this.Wniosek
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
      FEPZ2024: bool }

    member this.toChangeZrodlaZywnosci(teczka: int64): Commands.ZrodlaZywnosci =
        {
          Teczka = teczka
          Sieci = this.Sieci
          Bazarki = this.Bazarki
          Machfit = this.Machfit
          FEPZ2024 = this.FEPZ2024 }

    member this.toZrodlaZywnosci: ReadModels.ZrodlaZywnosci =
        { Sieci = this.Sieci
          Bazarki = this.Bazarki
          Machfit = this.Machfit
          FEPZ2024 = this.FEPZ2024 }

[<CLIMutable>]
type AdresyKsiegowosciForm =
    { NazwaOrganizacjiKsiegowanieDarowizn: string
      KsiegowanieAdres: string
      TelOrganProwadzacegoKsiegowosc: string }

    member this.toChangeAdresyKsiegowosci(teczka: int64): Commands.AdresyKsiegowosci =
        { Teczka = teczka
          NazwaOrganizacjiKsiegowanieDarowizn = this.NazwaOrganizacjiKsiegowanieDarowizn
          KsiegowanieAdres = this.KsiegowanieAdres
          TelOrganProwadzacegoKsiegowosc = this.TelOrganProwadzacegoKsiegowosc }

    member this.toAdresyKsiegowosci: ReadModels.AdresyKsiegowosci =
        { NazwaOrganizacjiKsiegowanieDarowizn = this.NazwaOrganizacjiKsiegowanieDarowizn
          KsiegowanieAdres = this.KsiegowanieAdres
          TelOrganProwadzacegoKsiegowosc = this.TelOrganProwadzacegoKsiegowosc }

