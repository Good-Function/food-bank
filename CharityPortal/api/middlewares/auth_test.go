package middlewares

import (
	"charity_portal/internal/auth"
	"context"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/stretchr/testify/assert"
)

func dummyProtectedHandler(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
	w.Write([]byte("protected"))
}

func dummyPublicHandler(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
	w.Write([]byte("public"))
}

func TestAuthMiddleware_LoggedOnly_ValidSession(t *testing.T) {
	middleware := NewAuthMiddleware(nil)

	user := &auth.UserClaims{Name: "John", Email: "john@example.com", Sub: "abc"}
	ctx := auth.SetUserContext(context.Background(), user)
	req := httptest.NewRequest(http.MethodGet, "/dashboard", nil).WithContext(ctx)
	rr := httptest.NewRecorder()

	handler := middleware.LoggedOnly(http.HandlerFunc(dummyProtectedHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusOK, rr.Code)
	assert.Equal(t, "protected", rr.Body.String())
}

func TestAuthMiddleware_LoggedOnly_InvalidSession(t *testing.T) {
	middleware := NewAuthMiddleware(nil)

	req := httptest.NewRequest(http.MethodGet, "/dashboard", nil)
	rr := httptest.NewRecorder()

	handler := middleware.LoggedOnly(http.HandlerFunc(dummyProtectedHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusFound, rr.Code)
	assert.Equal(t, "/", rr.Header().Get("Location"))
}

func TestAuthMiddleware_NotLogged_ValidSession(t *testing.T) {
	middleware := NewAuthMiddleware(nil)

	user := &auth.UserClaims{Name: "John", Email: "john@example.com", Sub: "abc"}
	ctx := auth.SetUserContext(context.Background(), user)
	req := httptest.NewRequest(http.MethodGet, "/", nil).WithContext(ctx)
	rr := httptest.NewRecorder()

	handler := middleware.NotLogged(http.HandlerFunc(dummyPublicHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusFound, rr.Code)
	assert.Equal(t, "/dashboard", rr.Header().Get("Location"))
}

func TestAuthMiddleware_NotLogged_InvalidSession(t *testing.T) {
	middleware := NewAuthMiddleware(nil)

	req := httptest.NewRequest(http.MethodGet, "/", nil)
	rr := httptest.NewRecorder()

	handler := middleware.NotLogged(http.HandlerFunc(dummyPublicHandler))
	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusOK, rr.Code)
	assert.Equal(t, "public", rr.Body.String())
}
