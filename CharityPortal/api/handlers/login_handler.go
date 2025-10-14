package handlers

import (
	"charity_portal/web"
	"net/http"
)

func LoginHandler(sessionManager *web.SessionManager) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		url := sessionManager.AuthProviderUrl(w)
		http.Redirect(w, r, url, http.StatusFound)
	}
}
