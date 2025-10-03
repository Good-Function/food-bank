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

type OrgInfo struct {
    Id                         *int64  `json:"id"`
    Name                       string `json:"name"`
}


type ReadKontaktyBy func(context.Context, int64) (Kontakty, error)
type ReadOrganizationIdByEmail func(context.Context, string) (OrgInfo, error)