module Organizations.Application.Commands

type DaneAdresowe =
    { Teczka: int64
      NazwaOrganizacjiPodpisujacejUmowe: string
      AdresRejestrowy: string
      NazwaPlacowkiTrafiaZywnosc: string
      AdresPlacowkiTrafiaZywnosc: string
      GminaDzielnica: string
      Powiat: string }

type ChangeDaneAdresowe = DaneAdresowe -> Async<unit>
