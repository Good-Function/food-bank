package api

import (
	"charity_portal/pkg/auth"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestRegisteredRoutes(t *testing.T) {
	authProvider, _ := auth.NewFakeAuth()
	router := newRouter(authProvider)
	if router == nil {
		assert.Fail(t, "Router should not be nil")
	}
	req := httptest.NewRequest("GET", "/dashboard", nil)
	rr := httptest.NewRecorder()

	router.ServeHTTP(rr, req)

	if rr.Code != http.StatusOK {
		assert.Fail(t, "Expected status code 200, got %d", rr.Code)
	}
}
