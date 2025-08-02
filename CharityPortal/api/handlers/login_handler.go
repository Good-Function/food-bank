package handlers

import (
	"charity_portal/internal/auth"
	"net/http"
)

func LoginHandler(sessionManager *auth.SessionManager) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		url := sessionManager.AuthProviderUrl(w)
		http.Redirect(w, r, url, http.StatusFound)
	}
}