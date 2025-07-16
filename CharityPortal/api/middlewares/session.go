package middlewares

import (
	"charity_portal/pkg/auth"
	"net/http"
)

type SessionMiddleware struct {
	authProvider   auth.AuthProvider
	appEnvironment string
}

func NewSessionMiddleware(authProvicer auth.AuthProvider, appEnvironment string) *SessionMiddleware {
	return &SessionMiddleware{
		authProvider:   authProvicer,
		appEnvironment: appEnvironment,
	}
}

func (am *SessionMiddleware) Session(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if am.appEnvironment == "development" {
			// In development mode, we skip session management for simplicity.
			sessionData := auth.UserClaims{
				Name:  "test",
				Email: "test@test.com",
				Sub:   "test-sub",
			}
			r = r.WithContext(auth.SetUserContext(r.Context(), &sessionData))
			h.ServeHTTP(w, r)
			return
		}
		cookie, err := r.Cookie("session")
		if err != nil {
			h.ServeHTTP(w, r)
			return
		}
		sessionData, err := am.authProvider.Decode("session", cookie.Value)
		if err != nil {
			h.ServeHTTP(w, r)
			return
		}
		if sessionData == nil {
			clearSessionCookie(w)
			h.ServeHTTP(w, r)
			return
		}
		r = r.WithContext(auth.SetUserContext(r.Context(), sessionData))
		h.ServeHTTP(w, r)
	})
}

func clearSessionCookie(w http.ResponseWriter) {
	http.SetCookie(w, &http.Cookie{
		Name:     "session",
		Value:    "",
		Path:     "/",
		HttpOnly: true,
		Secure:   true,
		MaxAge:   -1,
	})
}
