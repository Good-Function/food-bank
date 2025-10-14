package queries

import "context"

type Kontakty struct {
    WwwFacebook                string `json:"wwwFacebook"`
    Telefon                    string `json:"telefon"`
    Przedstawiciel             string `json:"przedstawiciel"`
    Kontakt                    string `json:"kontakt"`
    Email                      string `json:"email"`
    Dostepnosc                 string `json:"dostepnosc"`
    OsobaDoKontaktu            string `json:"osobaDoKontaktu"`
    TelefonOsobyKontaktowej    string `json:"telefonOsobyKontaktowej"`
    MailOsobyKontaktowej       string `json:"mailOsobyKontaktowej"`
    OsobaOdbierajacaZywnosc   string `json:"osobaOdbierajacaZywnosc"`
    TelefonOsobyOdbierajacej  string `json:"telefonOsobyOdbierajacej"`
}

type AdresyKsiegowosci struct {
    NazwaOrganizacjiKsiegowanieDarowizn string `json:"nazwaOrganizacjiKsiegowanieDarowizn"`
    KsiegowanieAdres                    string `json:"ksiegowanieAdres"`
    TelOrganProwadzacegoKsiegowosc      string `json:"telOrganProwadzacegoKsiegowosc"`
}

type ZrodlaZywnosci struct {
    Sieci             bool `json:"sieci"`
    Bazarki           bool `json:"bazarki"`
    Machfit           bool `json:"machfit"`
    FEPZ2024          bool `json:"fepz2024"`
    OdbiorKrotkiTermin bool `json:"odbiorKrotkiTermin"`
    TylkoNaszMagazyn  bool `json:"tylkoNaszMagazyn"`
}

type Beneficjenci struct {
    LiczbaBeneficjentow int    `json:"liczbaBeneficjentow"`
    Beneficjenci        string `json:"beneficjenci"`
}

type DaneAdresowe struct {
    NazwaOrganizacjiPodpisujacejUmowe string `json:"nazwaOrganizacjiPodpisujacejUmowe"`
    AdresRejestrowy                   string `json:"adresRejestrowy"`
    NazwaPlacowkiTrafiaZywnosc        string `json:"nazwaPlacowkiTrafiaZywnosc"`
    AdresPlacowkiTrafiaZywnosc        string `json:"adresPlacowkiTrafiaZywnosc"`
    GminaDzielnica                    string `json:"gminaDzielnica"`
    Powiat                            string `json:"powiat"`
}

type WarunkiPomocy struct {
    Kategoria              string `json:"kategoria"`
    RodzajPomocy           string `json:"rodzajPomocy"`
    SposobUdzielaniaPomocy string `json:"sposobUdzielaniaPomocy"`
    WarunkiMagazynowe      string `json:"warunkiMagazynowe"`
    HACCP                  bool   `json:"haccp"`
    Sanepid                bool   `json:"sanepid"`
    TransportOpis          string `json:"transportOpis"`
    TransportKategoria     string `json:"transportKategoria"`
}


type OrgInfo struct {
    Id                         *int64  `json:"id"`
    Name                       string `json:"name"`
}


type ReadKontaktyBy func(context.Context, int64) (Kontakty, error)
type UpdateKontaktyBy func(context.Context, int64, Kontakty) (error)

type ReadDaneAdresoweBy func(context.Context, int64) (DaneAdresowe, error)
type UpdateDaneAdresoweBy func(context.Context, int64, DaneAdresowe) (error)

type ReadAdresyKsiegowosciBy func(context.Context, int64) (AdresyKsiegowosci, error)
type UpdateAdresyKsiegowosciBy func(context.Context, int64, AdresyKsiegowosci) (error)

type ReadZrodlaZywnosciBy func(context.Context, int64) (ZrodlaZywnosci, error)
type UpdateZrodlaZywnosciBy func(context.Context, int64, ZrodlaZywnosci) (error)

type ReadBeneficjenciBy func(context.Context, int64) (Beneficjenci, error)
type UpdateBeneficjenciBy func(context.Context, int64, Beneficjenci) (error)

type ReadWarunkiPomocyBy func(context.Context, int64) (WarunkiPomocy, error)
type UpdateWarunkiPomocyBy func(context.Context, int64, WarunkiPomocy) (error)

type ReadOrganizationIdByEmail func(context.Context, string) (OrgInfo, error)