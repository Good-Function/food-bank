package middlewares

import (
	"charity_portal/pkg/auth"
	"charity_portal/tests/mocks"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/stretchr/testify/assert"
)

func dummyHandler(w http.ResponseWriter, r *http.Request) {
	user := auth.GetUserFromContext(r.Context())
	if user != nil {
		w.WriteHeader(http.StatusOK)
		_ = json.NewEncoder(w).Encode(user)
		return
	}
	w.WriteHeader(http.StatusUnauthorized)
}

func decodeUserClaimsResponse(t *testing.T, rr *httptest.ResponseRecorder) auth.UserClaims {
	var user auth.UserClaims
	err := json.NewDecoder(rr.Body).Decode(&user)
	assert.NoError(t, err)
	return user
}

func TestSessionMiddleware_Development(t *testing.T) {
	mockAuth := new(mocks.AuthProviderMock)
	middleware := NewSessionMiddleware(mockAuth, "development")

	req := httptest.NewRequest(http.MethodGet, "/", nil)
	rr := httptest.NewRecorder()

	handler := middleware.Session(http.HandlerFunc(dummyHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusOK, rr.Code)

	expectedUser := auth.UserClaims{
		Name:  "test",
		Email: "test@test.com",
		Sub:   "test-sub",
	}
	actualUser := decodeUserClaimsResponse(t, rr)
	assert.Equal(t, expectedUser, actualUser)
}

func TestSessionMiddleware_NoCookie(t *testing.T) {
	mockAuth := new(mocks.AuthProviderMock)
	middleware := NewSessionMiddleware(mockAuth, "production")

	req := httptest.NewRequest(http.MethodGet, "/", nil)
	rr := httptest.NewRecorder()

	handler := middleware.Session(http.HandlerFunc(dummyHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusUnauthorized, rr.Code)
}

func TestSessionMiddleware_InvalidCookie(t *testing.T) {
	mockAuth := new(mocks.AuthProviderMock)
	mockAuth.On("Decode", "session", "bad-cookie").Return(nil, assert.AnError)

	middleware := NewSessionMiddleware(mockAuth, "production")

	req := httptest.NewRequest(http.MethodGet, "/", nil)
	req.AddCookie(&http.Cookie{Name: "session", Value: "bad-cookie"})
	rr := httptest.NewRecorder()

	handler := middleware.Session(http.HandlerFunc(dummyHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusUnauthorized, rr.Code)
	mockAuth.AssertExpectations(t)
}

func TestSessionMiddleware_NilSessionData(t *testing.T) {
	mockAuth := new(mocks.AuthProviderMock)
	mockAuth.On("Decode", "session", "empty-session").Return(nil, nil)

	middleware := NewSessionMiddleware(mockAuth, "production")

	req := httptest.NewRequest(http.MethodGet, "/", nil)
	req.AddCookie(&http.Cookie{Name: "session", Value: "empty-session"})
	rr := httptest.NewRecorder()

	handler := middleware.Session(http.HandlerFunc(dummyHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusUnauthorized, rr.Code)
	mockAuth.AssertExpectations(t)
}

func TestSessionMiddleware_ValidSession(t *testing.T) {
	mockAuth := new(mocks.AuthProviderMock)
	sessionUser := &auth.UserClaims{
		Name:  "Valid User",
		Email: "valid@example.com",
		Sub:   "valid-sub",
	}
	mockAuth.On("Decode", "session", "valid-token").Return(sessionUser, nil)

	middleware := NewSessionMiddleware(mockAuth, "production")

	req := httptest.NewRequest(http.MethodGet, "/", nil)
	req.AddCookie(&http.Cookie{Name: "session", Value: "valid-token"})
	rr := httptest.NewRecorder()

	handler := middleware.Session(http.HandlerFunc(dummyHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusOK, rr.Code)

	actualUser := decodeUserClaimsResponse(t, rr)
	assert.Equal(t, *sessionUser, actualUser)
	mockAuth.AssertExpectations(t)
}
