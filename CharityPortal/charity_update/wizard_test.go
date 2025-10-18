package charityupdate

import (
	"charity_portal/web"
	"context"
	"fmt"
	"net/http"
	"net/http/httptest"
	"reflect"
	"strings"
	"testing"

	"charity_portal/charity_update/adapters"

	"github.com/PuerkitoBio/goquery"
	"github.com/stretchr/testify/assert"
)

func call(req *http.Request) *httptest.ResponseRecorder {
	orgId := int64(105)
	session := &web.SessionData{
		OrgID: &orgId,
		Email:   "test@example.com",
	}
	router := CreateRouter(Compose(Config{MockOperatorApi: true}))
	req = req.WithContext(
		context.WithValue(req.Context(), web.UserContextKey{}, session),
	)
	rr := httptest.NewRecorder()
	router.ServeHTTP(rr, req)
	return rr
}

func bodyToDoc(rr *httptest.ResponseRecorder) *goquery.Document {
	returnedHtml := rr.Body.String()
	doc, err := goquery.NewDocumentFromReader(strings.NewReader(returnedHtml))
	if err != nil {
		panic(err)
	}
	return doc
}


func TestWhenVisitingCharityUpdateThenShowsWizardWithAllSteps(t *testing.T) {
	// Arrange
	steps := map[string]bool{
		"Kontakty": true,
		"Beneficjenci": true,
		"Warunki udzielania pomocy": true,
		"Źrodła żywności": true,
		"Dane adresowe": true,
	}
	// Act
	recorded := call(httptest.NewRequest("GET", "/", nil))
	// Assert
	doc := bodyToDoc(recorded)
	assert.Contains(t, doc.Find("header").Text(), "🗓️ Aktualizacja danych")
	buttonsFound := doc.Find("button").FilterFunction(func(i int, s *goquery.Selection) bool {
		return steps[strings.TrimSpace(s.Text())]
	})
	assert.Equal(t, buttonsFound.Length(), len(steps), "Should find all expected buttons")
}

func TestViewKontakty(t *testing.T) {
	// Arrange + Act
	recorded := call(httptest.NewRequest("GET", "/kontakty-form", nil))
	// Assert
	doc := bodyToDoc(recorded)
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
	
	for _, name := range fields {
		val, _ := doc.Find(fmt.Sprintf("input[name='%s']", name)).Attr("value")
		expected := reflect.ValueOf(adapters.MockKontakty).FieldByName(name).Interface()
		assert.Equal(t, expected, val, "unexpected value for %s", name)
	}	
}

func TestViewDaneAdresowe(t *testing.T) {
	// Arrange + Act
	recorded := call(httptest.NewRequest("GET", "/dane-adresowe-form", nil))
	// Assert
	doc := bodyToDoc(recorded)
	fields := []string{
		"NazwaOrganizacjiPodpisujacejUmowe",
		"AdresRejestrowy",
		"NazwaPlacowkiTrafiaZywnosc",
		"AdresPlacowkiTrafiaZywnosc",
		"GminaDzielnica",
		"Powiat",
	}
	
	for _, name := range fields {
		val, _ := doc.Find(fmt.Sprintf("input[name='%s']", name)).Attr("value")
		expected := reflect.ValueOf(adapters.MockDaneAdresowe).FieldByName(name).Interface()
		assert.Equal(t, expected, val, "unexpected value for %s", name)
	}	
}

func TestViewZrodlaZywnosci(t *testing.T) {
	// Arrange + Act
	recorded := call(httptest.NewRequest("GET", "/zrodla-zywnosci-form", nil))
	// Assert
	doc := bodyToDoc(recorded)
	fields := []string{
		"Sieci",
		"Bazarki",
		"Machfit",
		"FEPZ2024",
		"OdbiorKrotkiTermin",
		"TylkoNaszMagazyn",
	}

	reflected := reflect.ValueOf(adapters.MockZrodlaZywnosci)
	for _, name := range fields {
		input := doc.Find(fmt.Sprintf("input[name='%s']", name))
		_, checked := input.Attr("checked")
		expected := reflected.FieldByName(name).Bool()
		assert.Equal(t, expected, checked)
	}
}

func TestViewBeneficjenci(t *testing.T) {	
	// Arrange + Act 
	recorded := call(httptest.NewRequest("GET", "/beneficjenci-form", nil))
	// Assert
	doc := bodyToDoc(recorded)
	liczba, _ := doc.Find(fmt.Sprintf("input[name='%s']", "LiczbaBeneficjentow")).Attr("value")
	assert.Equal(t, fmt.Sprintf("%d", adapters.Beneficjenci.LiczbaBeneficjentow), liczba)
	beneficjenci, _ := doc.Find("input[name='Beneficjenci']").Attr("value")
	assert.Equal(t, adapters.Beneficjenci.Beneficjenci, beneficjenci)
}

func TestViewWarunkiUdzielaniaPomocy(t *testing.T) {
	// Arrange + Act
	recorded := call(httptest.NewRequest("GET", "/warunki-udzielania-pomocy-form", nil))
	textFields := []string{
		"Kategoria",
		"RodzajPomocy",
		"SposobUdzielaniaPomocy",
		"WarunkiMagazynowe",
		"TransportOpis",
		"TransportKategoria",
	}
	// Assert
	doc := bodyToDoc(recorded)
	_, isHaccp := doc.Find("input[name='HACCP']").Attr("checked")
	_, isSanepid := doc.Find("input[name='Sanepid']").Attr("checked")
	assert.Equal(t, adapters.MockWarunkiPomocy.HACCP, isHaccp)
	assert.Equal(t, adapters.MockWarunkiPomocy.HACCP, isSanepid)
	for _, name := range textFields {
		val, _ := doc.Find(fmt.Sprintf("input[name='%s']", name)).Attr("value")
		expected := reflect.ValueOf(adapters.MockWarunkiPomocy).FieldByName(name).Interface()
		assert.Equal(t, expected, val, "unexpected value for %s", name)
	}
}
