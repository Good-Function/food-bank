package api

import (
	"charity_portal/api/middlewares"
	"charity_portal/web"
	"errors"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDefaultRouteShouldRedirectToCharityUpdae(t *testing.T) {
	router := newRouter(nil, nil, middlewares.ProtectFake, nil, nil)
	if router == nil {
		assert.Fail(t, "Router should not be nil")
	}
	req := httptest.NewRequest("GET", "/", nil)
	rr := httptest.NewRecorder()

	router.ServeHTTP(rr, req)

	if rr.Code != http.StatusSeeOther {
		assert.Fail(t, "Expected status code 200, got %d", rr.Code)
	}
 	assert.Equal(t, rr.Result().Header.Get("Location"), "/charity-update", "Expected redirect to /charity-update")
}

func TestRedirectUnauthenticatedToLogin(t *testing.T) {
	router := newRouter(nil, nil, middlewares.BuildProtect(func(r *http.Request) (*web.SessionData, error) {
		return nil, errors.New("no session")
	}), nil, nil)
	req := httptest.NewRequest("GET", "/charity-update/", nil)
	rr := httptest.NewRecorder()

	router.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusFound, rr.Code, "should redirect unauthenticated user")
	assert.Equal(t, "/login", rr.Header().Get("Location"), "should redirect to /login")	
}