package handlers

import (
	"charity_portal/internal/auth"
	"net/http"
)

type LoginHandler struct {
	authProvider auth.AuthProvider
}

func NewLoginHandler(auth auth.AuthProvider) *LoginHandler {
	return &LoginHandler{auth}
}

func (lh *LoginHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	//http.Redirect(w, r, lh.authProvider.GetLoginURL(), http.StatusFound)
}
