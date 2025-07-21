package storage

type OrganizationDTO struct {
	NIP                                 string          `db:"NIP"`
	Regon                               string          `db:"Regon"`
	KrsNr                               string          `db:"KrsNr"`
	FormaPrawna                         string          `db:"FormaPrawna"`
	OPP                                 bool            `db:"OPP"`
	NazwaOrganizacjiPodpisujacejUmowe   string          `db:"NazwaOrganizacjiPodpisujacejUmowe"`
	AdresRejestrowy                     string          `db:"AdresRejestrowy"`
	NazwaPlacowkiTrafiaZywnosc          string          `db:"NazwaPlacowkiTrafiaZywnosc"`
	AdresPlacowkiTrafiaZywnosc          string          `db:"AdresPlacowkiTrafiaZywnosc"`
	GminaDzielnica                      string          `db:"GminaDzielnica"`
	Powiat                              string          `db:"Powiat"`
	NazwaOrganizacjiKsiegowanieDarowizn string          `db:"NazwaOrganizacjiKsiegowanieDarowizn"`
	KsiegowanieAdres                    string          `db:"KsiegowanieAdres"`
	TelOrganProwadzacegoKsiegowosc      string          `db:"TelOrganProwadzacegoKsiegowosc"`
	WwwFacebook                         string          `db:"WwwFacebook"`
	Telefon                             string          `db:"Telefon"`
	Przedstawiciel                      string          `db:"Przedstawiciel"`
	Kontakt                             string          `db:"Kontakt"`
	Email                               string          `db:"Email"`
	Dostepnosc                          string          `db:"Dostepnosc"`
	OsobaDoKontaktu                     string          `db:"OsobaDoKontaktu"`
	TelefonOsobyKontaktowej             string          `db:"TelefonOsobyKontaktowej"`
	MailOsobyKontaktowej                string          `db:"MailOsobyKontaktowej"`
	OsobaOdbierajacaZywnosc             string          `db:"OsobaOdbierajacaZywnosc"`
	TelefonOsobyOdbierajacej            string          `db:"TelefonOsobyOdbierajacej"`
	LiczbaBeneficjentow                 int             `db:"LiczbaBeneficjentow"`
	Beneficjenci                        string          `db:"Beneficjenci"`
	Sieci                               bool            `db:"Sieci"`
	Bazarki                             bool            `db:"Bazarki"`
	Machfit                             bool            `db:"Machfit"`
	FEPZ2024                            bool            `db:"FEPZ2024"`
	OdbiorKrotkiTermin                  bool            `db:"OdbiorKrotkiTermin"`
	TylkoNaszMagazyn                    bool            `db:"TylkoNaszMagazyn"`
	Kategoria                           string          `db:"Kategoria"`
	RodzajPomocy                        string          `db:"RodzajPomocy"`
	SposobUdzielaniaPomocy              string          `db:"SposobUdzielaniaPomocy"`
	WarunkiMagazynowe                   string          `db:"WarunkiMagazynowe"`
	HACCP                               bool            `db:"HACCP"`
	Sanepid                             bool            `db:"Sanepid"`
	TransportOpis                       string          `db:"TransportOpis"`
	TransportKategoria                  string          `db:"TransportKategoria"`
}
