package charityupdate

import (
	"charity_portal/web"
	"charity_portal/charity_update/adapters"
	"context"
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
	orgId := int64(105)
	session := &web.SessionData{
		OrgID: &orgId,
		Email:   "test@example.com",
	}
	req := httptest.NewRequest(http.MethodGet, "/", nil)
	req = req.WithContext(
		context.WithValue(req.Context(), web.UserContextKey{}, session),
	)
	// Act
	router.ServeHTTP(rr, req)
	// Assert
	returnedHtml := rr.Body.String()
	assert.Equal(t, 200, rr.Code)
	doc, err := goquery.NewDocumentFromReader(strings.NewReader(returnedHtml))
	assert.NoError(t, err, "Should not return an error when parsing HTML")
	assert.Contains(t, doc.Text(), fmt.Sprintf("%+v\n", adapters.MockKontakty))
}