package middlewares

import (
	"charity_portal/internal/auth"
	"net/http"
)

type Auth2Middleware struct {
	authProvider auth.AuthProvider
}

func NewAuth2Middleware(authProvicer auth.AuthProvider) *Auth2Middleware {
	return &Auth2Middleware{
		authProvider: authProvicer,
	}
}

func (am *Auth2Middleware) Protect(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		cookie, err := r.Cookie("session")
		if err != nil {
			http.Error(w, "Unauthorized", http.StatusUnauthorized)
			return
		}

		sessionData, err := am.authProvider.Decode("session", cookie.Value)
		if err != nil || sessionData == nil {
			clearSessionCookie(w)
			http.Redirect(w, r, "/login", http.StatusFound)
			return
		}

		r = r.WithContext(auth.SetUserContext(r.Context(), sessionData))
		h.ServeHTTP(w, r)
	})
}
