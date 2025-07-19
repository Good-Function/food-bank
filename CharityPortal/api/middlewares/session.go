package middlewares

import (
	"charity_portal/pkg/auth"
	"net/http"
)

type SessionMiddleware struct {
	authProvider auth.AuthProvider
}

func NewSessionMiddleware(authProvicer auth.AuthProvider) *SessionMiddleware {
	return &SessionMiddleware{
		authProvider: authProvicer,
	}
}

func (am *SessionMiddleware) Session(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		var cookieVal string
		if cookie, err := r.Cookie("session"); err == nil {
			cookieVal = cookie.Value
		}
		var sessionData *auth.UserClaims
		var err error

		if sessionData, err = am.authProvider.Decode("session", cookieVal); err == nil {
			r = r.WithContext(auth.SetUserContext(r.Context(), sessionData))
		}

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
