package middleware

import (
	"charity_portal/pkg/auth"
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

func (am *AuthMiddleware) Auth(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		cookie, err := r.Cookie("session")
		if err != nil {
			http.Error(w, "Unauthorized", http.StatusUnauthorized)
			return
		}

		sessionData, err := am.authProvider.Decode("session", cookie.Value)
		if err != nil {
			http.Error(w, "Unauthorized", http.StatusUnauthorized)
			return
		}

		if sessionData == nil {
			http.Error(w, "Unauthorized", http.StatusUnauthorized)
			return
		}
		r = r.WithContext(auth.SetUserContext(r.Context(), sessionData))
		h.ServeHTTP(w, r)
	})
}
