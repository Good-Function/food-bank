package handlers

import (
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"github.com/stretchr/testify/assert"
	"golang.org/x/net/html"
)

func TestHomeHandler(t *testing.T) {
	handler := NewHomeHandler()
	rr := httptest.NewRecorder()

	req, err := http.NewRequest("GET", "/", nil)
	if err != nil {
		assert.Fail(t, "Failed to create request: %v", err)
	}

	handler.ServeHTTP(rr, req)

	if rr.Code != http.StatusOK {
		assert.Fail(t, "Expected status code 200, got %d", rr.Code)
	}

	// Check if the response contains expected content
	resParsed, err := html.Parse(strings.NewReader(rr.Body.String()))
	assert.NoError(t, err, "Failed to parse response body as HTML")

	var found bool
	var f func(*html.Node)
	f = func(n *html.Node) {
		if n.Type == html.ElementNode && n.Data == "form" {
			for _, attr := range n.Attr {
				if attr.Key == "action" && attr.Val == "/login" {
					found = true
					return
				}
			}
		}
		for c := n.FirstChild; c != nil; c = c.NextSibling {
			f(c)
		}
	}
	f(resParsed)
	assert.True(t, found, "Expected to find a form with action '/login' in the response body")
}
