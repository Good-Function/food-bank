package auth

import (
	"charity_portal/config"
	"context"
	"fmt"
	"net/url"

	"github.com/coreos/go-oidc/v3/oidc"
	"golang.org/x/oauth2"
)

type AuthProvider interface {
	GetLoginURL() string
	ExchangeToken(ctx context.Context, url url.Values) (*UserClaims, error)
}

type Auth struct {
	provider     *oidc.Provider
	oidcConfig   *oidc.Config
	oauth2Config *oauth2.Config
	verifier     *oidc.IDTokenVerifier
}

type UserClaims struct {
	Name  string `json:"name"`
	Email string `json:"email"`
	Sub   string `json:"sub"`
}

func NewAuth(cfg *config.Auth) (*Auth, error) {
	ctx := context.Background()
	providerURL := fmt.Sprintf("https://%s.ciamlogin.com/%s/v2.0", cfg.TenantID, cfg.TenantID)
	provider, err := oidc.NewProvider(ctx, providerURL)
	if err != nil {
		return nil, fmt.Errorf("Cannot create oidc provider; %w", err)
	}

	oidcConfig := &oidc.Config{ClientID: cfg.ClientID}
	verifier := provider.Verifier(oidcConfig)

	oauth2Config := &oauth2.Config{
		ClientID:     cfg.ClientID,
		ClientSecret: cfg.ClientSecret,
		RedirectURL:  cfg.RedirectURL,
		Endpoint:     provider.Endpoint(),
		Scopes:       []string{oidc.ScopeOpenID, "profile", "email"},
	}

	return &Auth{
		provider:     provider,
		oidcConfig:   oidcConfig,
		oauth2Config: oauth2Config,
		verifier:     verifier,
	}, nil
}

func (a *Auth) GetLoginURL() string {
	return a.oauth2Config.AuthCodeURL("dupa", oauth2.SetAuthURLParam("p", "user_signup"))
}

func (a *Auth) ExchangeToken(ctx context.Context, url url.Values) (*UserClaims, error) {
	code := url.Get("code")
	token, err := a.oauth2Config.Exchange(ctx, code)
	if err != nil {
		return nil, fmt.Errorf("error exchanging token: %w", err)
	}

	rawIDToken, ok := token.Extra("id_token").(string)
	if !ok {
		return nil, fmt.Errorf("missing id_token in token exchange response")
	}

	idToken, err := a.verifier.Verify(ctx, rawIDToken)
	if err != nil {
		return nil, fmt.Errorf("invalid id_token: %w", err)
	}

	var user UserClaims
	if err := idToken.Claims(&user); err != nil {
		return nil, fmt.Errorf("error parsing id_token claims: %w", err)
	}
	return &user, nil
}
