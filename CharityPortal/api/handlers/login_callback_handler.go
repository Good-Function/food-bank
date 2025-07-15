package handlers

import (
	"charity_portal/pkg/auth"
	"log/slog"
	"net/http"
	"time"
)

type LoginCallbackHandler struct {
	authProvider auth.AuthProvider
}

func NewLoginCallbackHandler(auth auth.AuthProvider) *LoginCallbackHandler {
	return &LoginCallbackHandler{auth}
}

func (lh *LoginCallbackHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	slog.With("handler", "LoginCallbackHandler").Info("Processing login callback")
	user, err := lh.authProvider.ExchangeToken(r.Context(), r.URL.Query())
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	sessionData := map[string]string{
		"email": user.Email,
		"name":  user.Name,
		"sub":   user.Sub,
	}

	encodedCookie, err := lh.authProvider.Encode("session", sessionData)

	if err != nil {
		slog.Error("Failed to encode session data", "error", err)
		http.Error(w, "Failed to encode session data", http.StatusInternalServerError)
		return
	}

	http.SetCookie(w, &http.Cookie{
		Name:     "session",
		Value:    encodedCookie,
		Path:     "/",
		HttpOnly: true,
		Secure:   true,
		SameSite: http.SameSiteLaxMode,
		Expires:  time.Now().Add(24 * time.Hour),
	})

	http.Redirect(w, r, "/dashboard", http.StatusFound)
}
