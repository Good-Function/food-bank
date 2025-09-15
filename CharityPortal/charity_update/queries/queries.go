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


type ReadKontaktyBy func(context.Context, string) (Kontakty, error)