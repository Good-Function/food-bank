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