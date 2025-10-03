package adapters

import (
	"charity_portal/charity_update/queries"
	"fmt"
)

var MockKontakty = queries.Kontakty {
	Kontakt: "A nie wiem",
	WwwFacebook: "http://onet.pl",
	Telefon: "726 221 122",
	Przedstawiciel: "Marcin Golenia",
	Email: "marcin.teodor@gmail.com",
	Dostepnosc: "Pn-Pt 9:00 - 12:00",
	OsobaDoKontaktu: "Marcin Gienia",
	TelefonOsobyKontaktowej: "+48 200",
	MailOsobyKontaktowej: "teresa@bezdresa.pl",
	OsobaOdbierajacaZywnosc: "Sam odbiore",
	TelefonOsobyOdbierajacej: "726 221 122",
}


var CallOperatorMock CallOperator = func(method, url string, in any, out any) error {
	switch v := out.(type) {
	case *queries.Kontakty:
		*v = MockKontakty
		return nil
	default:
		return fmt.Errorf("unsupported type in mock: %T", out)
	}
}
