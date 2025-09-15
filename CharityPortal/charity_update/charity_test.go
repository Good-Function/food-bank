package charityupdate

import (
	"charity_portal/charity_update/adapters"
	"fmt"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"github.com/PuerkitoBio/goquery"
	"github.com/stretchr/testify/assert"
)

func TestWhenVisitingCharityUpdateThenShowsKontakty(t *testing.T) {
	// Arrange
	router := CreateRouter(Compose(Config{MockOperatorApi: true}))
	rr := httptest.NewRecorder()
	// Act
	router.ServeHTTP(rr, httptest.NewRequest(http.MethodGet, "/", nil))
	// Assert
	returnedHtml := rr.Body.String()
	assert.Equal(t, 200, rr.Code)
	doc, err := goquery.NewDocumentFromReader(strings.NewReader(returnedHtml))
	assert.NoError(t, err, "Should not return an error when parsing HTML")
	assert.Contains(t, doc.Text(), fmt.Sprintf("%+v\n", adapters.MockKontakty))
}