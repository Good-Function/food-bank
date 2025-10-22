package adapters

import (
	"charity_portal/charity_update/operator_api"
	"fmt"
)

var Beneficjenci = operator_api.Beneficjenci{
	LiczbaBeneficjentow: 150,
	Beneficjenci:        "Osoby bezdomne, rodziny wielodzietne",
}

var MockKontakty = operator_api.Kontakty{
	Kontakt:                  "A nie wiem",
	WwwFacebook:              "http://onet.pl",
	Telefon:                  "726 221 122",
	Przedstawiciel:           "Marcin Golenia",
	Email:                    "marcin.teodor@gmail.com",
	Dostepnosc:               "Pn-Pt 9:00 - 12:00",
	OsobaDoKontaktu:          "Marcin Gienia",
	TelefonOsobyKontaktowej:  "+48 200",
	MailOsobyKontaktowej:     "teresa@bezdresa.pl",
	OsobaOdbierajacaZywnosc:  "Sam odbiore",
	TelefonOsobyOdbierajacej: "726 221 122",
}

var MockDaneAdresowe = operator_api.DaneAdresowe{
	NazwaOrganizacjiPodpisujacejUmowe: "Fundacja Testowa",
	AdresRejestrowy:                   "ul. Testowa 12, 00-001 Warszawa",
	NazwaPlacowkiTrafiaZywnosc:        "Magazyn Główny",
	AdresPlacowkiTrafiaZywnosc:        "ul. Magazynowa 5, 00-002 Warszawa",
	GminaDzielnica:                    "Mokotów",
	Powiat:                            "Warszawa",
}

var MockZrodlaZywnosci = operator_api.ZrodlaZywnosci{
	Sieci:              true,
	Bazarki:            false,
	Machfit:            true,
	FEPZ2024:           false,
	OdbiorKrotkiTermin: true,
	TylkoNaszMagazyn:   false,
}

var MockWarunkiPomocy = operator_api.WarunkiPomocy{
	Kategoria:              "Organizacja pozarządowa",
	RodzajPomocy:           "Dystrybucja żywności",
	SposobUdzielaniaPomocy: "Bezpośrednia dystrybucja",
	WarunkiMagazynowe:      "Posiadamy chłodnię",
	HACCP:                  true,
	Sanepid:                true,
	TransportOpis:          "Posiadamy własny transport",
	TransportKategoria:     "Samochód dostawczy",
}

func mockGet(out any) error {
	switch v := out.(type) {
	case *operator_api.Kontakty:
		*v = MockKontakty
		return nil
	case *operator_api.DaneAdresowe:
		*v = MockDaneAdresowe
		return nil
	case *operator_api.ZrodlaZywnosci:
		*v = MockZrodlaZywnosci
		return nil
	case *operator_api.WarunkiPomocy:
		*v = MockWarunkiPomocy
		return nil
	case *operator_api.Beneficjenci:
		*v = Beneficjenci
		return nil
	default:
		return fmt.Errorf("unsupported type in mock: %T", out)
	}
}

func mockPut(in any) error {
	switch v := in.(type) {
	case operator_api.Kontakty:
		MockKontakty = v
		return nil
	case operator_api.DaneAdresowe:
		MockDaneAdresowe = v
		return nil
	case operator_api.ZrodlaZywnosci:
		MockZrodlaZywnosci = v
		return nil
	case operator_api.WarunkiPomocy:
		MockWarunkiPomocy = v
		return nil
	case operator_api.Beneficjenci:
		Beneficjenci = v
		return nil
	default:
		return fmt.Errorf("unsupported type in mock: %T", in)
	}
}

var CallOperatorMock CallOperator = func(_,_ string, in any, out any) error {
	if out != nil {
		return mockGet(out)
	} else {
		return mockPut(in)
	}
}
