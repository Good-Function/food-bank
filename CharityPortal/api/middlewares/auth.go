package middlewares

import (
	"charity_portal/internal/auth"
	"net/http"
)

type AuthMiddleware struct {
	authProvider auth.AuthProvider
}

func NewAuthMiddleware(authProvicer auth.AuthProvider) *AuthMiddleware {
	return &AuthMiddleware{
		authProvider: authProvicer,
	}
}

func (am *AuthMiddleware) LoggedOnly(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if !auth.IsSessionValid(r.Context()) {
			clearSessionCookie(w)
			http.Redirect(w, r, "/", http.StatusFound)
			return
		}
		h.ServeHTTP(w, r)
	})
}

func (am *AuthMiddleware) NotLogged(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if auth.IsSessionValid(r.Context()) {
			http.Redirect(w, r, "/dashboard", http.StatusFound)
			return
		}
		h.ServeHTTP(w, r)
	})
}
