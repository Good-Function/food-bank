package handlers

import (
	"charity_portal/internal/auth"
	"net/http"

	"golang.org/x/oauth2"
)

func LoginHandler(authConfig *oauth2.Config, sessionManager *auth.SessionManager) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		state := auth.GenerateRandomState()
		_ = sessionManager.WriteStateCookie(w, state)
		http.Redirect(w, r, authConfig.AuthCodeURL(state), http.StatusFound)
	}
}