package handlers

import (
	"log/slog"
	"net/http"

	"charity_portal/charity_update/operator_api"
	"charity_portal/web"

	"github.com/coreos/go-oidc/v3/oidc"
)

type LoginCallbackHandler struct {
	verifier       *oidc.IDTokenVerifier
	sessionManager *web.SessionManager
	readOrgId operator_api.ReadOrganizationIdByEmail
}

func NewLoginCallbackHandler(
	verifier *oidc.IDTokenVerifier,
	sessionManager *web.SessionManager,
	readOrgId operator_api.ReadOrganizationIdByEmail,
) *LoginCallbackHandler {
	return &LoginCallbackHandler{verifier, sessionManager, readOrgId}
}

func (lh *LoginCallbackHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	slog.With("handler", "LoginCallbackHandler").Info("Processing login callback")
	ctx := r.Context()

	if err := lh.sessionManager.ValidateState(r); err != nil {
		http.Error(w, "Invalid state", http.StatusBadRequest)
		return
	}

	token, err := lh.sessionManager.ExchangeCodeForToken(ctx, r.FormValue("code"))
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

	orgInfo, err := lh.readOrgId(ctx, claims.Email)
	if err != nil {
		http.Error(w, "Failed to resolve organization", http.StatusInternalServerError)
		return
	}

	slog.With("handler", "LoginCallbackHandler").Info("session for org with id created", "orgId", orgInfo.Id, "email", claims.Email)
	_ = lh.sessionManager.WriteSession(w, claims.Email, orgInfo.Id)
	http.Redirect(w, r, "/", http.StatusFound)
}
