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
