package charityupdate

import (
	"charity_portal/web"
	"context"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

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
	recorded := call(httptest.NewRequest("GET", "/charity-update/", nil))
	// Assert
	doc := bodyToDoc(recorded)
	assert.Contains(t, doc.Find("header").Text(), "🗓️ Aktualizacja danych")
	buttonsFound := doc.Find("button").FilterFunction(func(i int, s *goquery.Selection) bool {
		return steps[strings.TrimSpace(s.Text())]
	})
	assert.Equal(t, buttonsFound.Length(), len(steps), "Should find all expected buttons")
}