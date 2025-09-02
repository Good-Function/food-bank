package api

import (
	"charity_portal/api/middlewares"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestRegisteredRoutes(t *testing.T) {
	router := newRouter(nil, nil, middlewares.ProtectFake, nil)
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

func TestUnauthorizedAccessRoutes(t *testing.T) {
	router := NewUnauthenticatedRouter()

	tests := []struct {
		name           string
		method         string
		path           string
		expectedStatus int
		redirectURL    string
	}{
		{
			name:           "unauthorized access to /dashboard should redirect to login",
			method:         http.MethodGet,
			path:           "/dashboard",
			expectedStatus: http.StatusFound,
			redirectURL:    "/login",
		},
		{
			name:           "unauthorized access to / should return 302",
			method:         http.MethodGet,
			path:           "/",
			expectedStatus: http.StatusFound,
			redirectURL:    "/login",
		},
		{
			name:           "unauthorized logout should redirect",
			method:         http.MethodPost,
			path:           "/logout",
			expectedStatus: http.StatusFound,
			redirectURL:    "/",
		},
		{
			name:           "unauthorized logout should redirect",
			method:         http.MethodPost,
			path:           "/logout",
			expectedStatus: http.StatusFound,
			redirectURL:    "/",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			req := httptest.NewRequest(tt.method, tt.path, nil)
			rr := httptest.NewRecorder()
			router.ServeHTTP(rr, req)
			assert.Equal(t, tt.expectedStatus, rr.Code)
			if tt.expectedStatus == http.StatusFound {
				assert.Equal(t, tt.redirectURL, rr.Header().Get("Location"))
			} else {
				assert.NotEmpty(t, rr.Body.String(), "Expected response body to not be empty")
			}
		})
	}
}

func TestAuthorizedAccessRoutes(t *testing.T) {
	router := NewAuthenticatedRouter()
	tests := []struct {
		name           string
		method         string
		path           string
		expectedStatus int
		redirectURL    string
	}{
		{
			name:           "authorized access to /dashboard should return 200",
			method:         http.MethodGet,
			path:           "/dashboard",
			expectedStatus: http.StatusOK,
		},
		{
			name:           "authorized access to /data-confirmation should return 200",
			method:         http.MethodPost,
			path:           "/data-confirmation",
			expectedStatus: http.StatusOK,
		},
		{
			name:           "authorized access to /logout should redirect",
			method:         http.MethodPost,
			path:           "/logout",
			expectedStatus: http.StatusFound,
			redirectURL:    "/",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			rr := httptest.NewRecorder()
			router.ServeHTTP(rr, httptest.NewRequest(tt.method, tt.path, nil))
			assert.Equal(t, tt.expectedStatus, rr.Code)
			if tt.expectedStatus == http.StatusFound {
				assert.Equal(t, tt.redirectURL, rr.Header().Get("Location"))
			} else {
				assert.NotEmpty(t, rr.Body.String(), "Expected response body to not be empty")
			}
		})
	}
}
