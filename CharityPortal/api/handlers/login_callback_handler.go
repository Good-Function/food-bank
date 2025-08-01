package handlers

import (
	"log/slog"
	"net/http"

	"charity_portal/internal/auth"

	"github.com/coreos/go-oidc/v3/oidc"
	"golang.org/x/oauth2"
)

type LoginCallbackHandler struct {
	authConfig *oauth2.Config
	verifier *oidc.IDTokenVerifier
    sessionManager *auth.SessionManager
}

func NewLoginCallbackHandler(
    auth *oauth2.Config, 
    verifier *oidc.IDTokenVerifier,
    sessionManager *auth.SessionManager,
    ) *LoginCallbackHandler {
	return &LoginCallbackHandler{auth, verifier, sessionManager}
}

func (lh *LoginCallbackHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	slog.With("handler", "LoginCallbackHandler").Info("Processing login callback")
	ctx := r.Context()

    if err := lh.sessionManager.ValidateState(r); err != nil {
        http.Error(w, "Invalid state", http.StatusBadRequest)
        return
    }

    token, err := lh.authConfig.Exchange(ctx, r.FormValue("code"))
    if err != nil {
        http.Error(w, "Failed to exchange token", http.StatusInternalServerError)
        return
    }

    rawIDToken, ok := token.Extra("id_token").(string)
    if !ok {
        http.Error(w, "Missing id_token", http.StatusInternalServerError)
        return
    }

    idToken, err := lh.verifier.Verify(ctx, rawIDToken)
    if err != nil {
        http.Error(w, "Invalid ID token", http.StatusInternalServerError)
        return
    }

    var claims struct {
        Email string `json:"email"`
    }

    if err := idToken.Claims(&claims); err != nil {
        http.Error(w, "Invalid claims", http.StatusInternalServerError)
        return
    }

    _ = lh.sessionManager.WriteSession(w, claims.Email)
    http.Redirect(w, r, "/dashboard", http.StatusFound)
}
