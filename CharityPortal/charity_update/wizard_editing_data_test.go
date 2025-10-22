package charityupdate

import (
	"fmt"
	"log"
	"math/rand"
	"net/http/httptest"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
)

func TestEditKontakty(t *testing.T) {
	// Arrange
	fields := []string{
		"WwwFacebook",
		"Email",
		"Kontakt",
		"Telefon",
		"OsobaDoKontaktu",
		"MailOsobyKontaktowej",
		"TelefonOsobyKontaktowej",
		"Dostepnosc",
		"OsobaOdbierajacaZywnosc",
		"TelefonOsobyOdbierajacej",
		"Przedstawiciel",
	}
	randomGUID := uuid.New().String()
	formValues := make(map[string][]string)
	for _, name := range fields {
		formValues[name] = []string{randomGUID}
	}
	// Act
	req := httptest.NewRequest("PUT", "/kontakty-form", nil)
	req.PostForm = formValues
	editResponse := call(req)
	// Assert
	assert.Equal(t, 200, editResponse.Code)
	kontaktyResponse := call(httptest.NewRequest("GET", "/kontakty-form", nil))
	doc := bodyToDoc(kontaktyResponse)
	for _, name := range fields {
		val, _ := doc.Find(fmt.Sprintf("input[name='%s']", name)).Attr("value")
		assert.Equal(t, randomGUID, val, "unexpected value for %s", name)
	}	
}

func TestEditBeneficjenci(t *testing.T) {
	// Arrange
	randomGUID := uuid.New().String()
	randomInt :=  rand.Intn(100)
	// Act
	req := httptest.NewRequest("PUT", "/beneficjenci-form", nil)
	req.PostForm = map[string][]string{
		"LiczbaBeneficjentow": {fmt.Sprintf("%d", randomInt)},
		"Beneficjenci":        {randomGUID},
	}
	call(req)
	// Assert
	recorded := call(httptest.NewRequest("GET", "/beneficjenci-form", nil))
	doc := bodyToDoc(recorded)
	liczba, _ := doc.Find(fmt.Sprintf("input[name='%s']", "LiczbaBeneficjentow")).Attr("value")
	assert.Equal(t, fmt.Sprintf("%d", randomInt), liczba)
	beneficjenci, _ := doc.Find("input[name='Beneficjenci']").Attr("value")
	assert.Equal(t, randomGUID, beneficjenci)
}

func TestEditDaneAdresowe(t *testing.T) {
	// Arrange
	fields := []string{
		"NazwaOrganizacjiPodpisujacejUmowe",
		"AdresRejestrowy",
		"NazwaPlacowkiTrafiaZywnosc",
		"AdresPlacowkiTrafiaZywnosc",
		"GminaDzielnica",
		"Powiat",
	}
	randomGUID := uuid.New().String()
	formValues := make(map[string][]string)
	for _, name := range fields {
		formValues[name] = []string{randomGUID}
	}
	// Act
	req := httptest.NewRequest("PUT", "/dane-adresowe-form", nil)
	req.PostForm = formValues
	editResponse := call(req)
	// Assert
	daneAdresoweResponse := call(httptest.NewRequest("GET", "/dane-adresowe-form", nil))
	assert.Equal(t, 200, editResponse.Code)
	doc := bodyToDoc(daneAdresoweResponse)
	for _, name := range fields {
		val, _ := doc.Find(fmt.Sprintf("input[name='%s']", name)).Attr("value")
		assert.Equal(t, randomGUID, val, "unexpected value for %s", name)
	}	
}

func TestEditZrodlaZywnosci(t *testing.T) {
	// Arrange
	fields := []string{
		"Sieci",
		"Bazarki",
		"Machfit",
		"FEPZ2024",
		"OdbiorKrotkiTermin",
		"TylkoNaszMagazyn",
	}
	formValues := make(map[string][]string)
	for _, name := range fields {
		formValues[name] = []string{"on"}
	}
	// Act
	req := httptest.NewRequest("PUT", "/zrodla-zywnosci-form", nil)
	req.PostForm = formValues
	editResponse := call(req)
	// Assert
	zrodlaZywnosciResponse := call(httptest.NewRequest("GET", "/zrodla-zywnosci-form", nil))
	assert.Equal(t, 200, editResponse.Code)
	doc := bodyToDoc(zrodlaZywnosciResponse)
	for _, name := range fields {
		input := doc.Find(fmt.Sprintf("input[name='%s']", name))
		_, checked := input.Attr("checked")
		assert.Equal(t, true, checked, "unexpected value for %s", name)
	}	
}

func TestEditWarunkiUdzielaniaPomocy(t *testing.T) {
	// Arrange
	textFields := []string{
		"Kategoria",
		"RodzajPomocy",
		"SposobUdzielaniaPomocy",
		"WarunkiMagazynowe",
		"TransportOpis",
		"TransportKategoria",
	}
	randomGUID := uuid.New().String()
	formValues := make(map[string][]string)
	for _, name := range textFields {
		formValues[name] = []string{randomGUID}
	}
	formValues["HACCP"] = []string{"on"}
	formValues["Sanepid"] = []string{"on"}
	// Act
	req := httptest.NewRequest("PUT", "/warunki-udzielania-pomocy-form", nil)
	req.PostForm = formValues
	editResponse := call(req)
	// Assert
	assert.Equal(t, 200, editResponse.Code)
	warunkiResponse := call(httptest.NewRequest("GET", "/warunki-udzielania-pomocy-form", nil))
	doc := bodyToDoc(warunkiResponse)
	html, err := doc.Html()
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(html)
	doc.Contents()
	for _, name := range textFields {
		val, _ := doc.Find(fmt.Sprintf("input[name='%s']", name)).Attr("value")
		assert.Equal(t, randomGUID, val, "unexpected value for %s", name)
	}	
	_, isHaccp := doc.Find("input[name='HACCP']").Attr("checked")
	_, isSanepid := doc.Find("input[name='Sanepid']").Attr("checked")
	assert.Equal(t, true, isHaccp)
	assert.Equal(t, true, isSanepid)
}

