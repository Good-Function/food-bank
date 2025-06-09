package handlers

import (
	"charity_portal/pkg/auth"
	"net/http"
)

type LoginCallbackHandler struct {
	authProvider auth.AuthProvider
}

func NewLoginCallbackHandler(auth auth.AuthProvider) *LoginCallbackHandler {
	return &LoginCallbackHandler{auth}
}

func (lh *LoginCallbackHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	user, err := lh.authProvider.ExchangeToken(r.Context(), r.URL.Query())
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	w.Write([]byte(`{"name":"` + user.Name + `", "email":"` + user.Email + `"}`))
}
