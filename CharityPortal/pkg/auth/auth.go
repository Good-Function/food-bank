package auth

import (
	"charity_portal/config"
	"context"
	"fmt"
	"net/url"

	"github.com/coreos/go-oidc/v3/oidc"
	"github.com/gorilla/securecookie"
	"golang.org/x/oauth2"
)

type AuthProvider interface {
	GetLoginURL() string
	ExchangeToken(ctx context.Context, url url.Values) (*UserClaims, error)
	Encode(name string, value interface{}) (string, error)
	Decode(name, value string) (*UserClaims, error)
}

type Auth struct {
	provider     *oidc.Provider
	oidcConfig   *oidc.Config
	oauth2Config *oauth2.Config
	verifier     *oidc.IDTokenVerifier
	state        string
	securecookie *securecookie.SecureCookie
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

	securecookie := securecookie.New([]byte(cfg.HashKey), []byte(cfg.BlockKey))

	return &Auth{
		provider:     provider,
		oidcConfig:   oidcConfig,
		oauth2Config: oauth2Config,
		verifier:     verifier,
		state:        cfg.State,
		securecookie: securecookie,
	}, nil
}

func (a *Auth) GetLoginURL() string {
	return a.oauth2Config.AuthCodeURL(a.state, oauth2.SetAuthURLParam("p", "user_signup"))
}

func (a *Auth) ExchangeToken(ctx context.Context, url url.Values) (*UserClaims, error) {
	if url.Get("state") != a.state {
		return nil, fmt.Errorf("state mismatch")
	}
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

func (a *Auth) Encode(name string, value interface{}) (string, error) {
	encoded, err := a.securecookie.Encode(name, value)
	if err != nil {
		return "", err
	}
	return encoded, nil
}

func (a *Auth) Decode(name, value string) (*UserClaims, error) {
	var decoded map[string]string
	if err := a.securecookie.Decode(name, value, &decoded); err != nil {
		return nil, fmt.Errorf("error decoding cookie: %w", err)
	}
	if decoded == nil {
		return nil, fmt.Errorf("decoded value is nil")
	}

	user := &UserClaims{
		Name:  decoded["name"],
		Email: decoded["email"],
		Sub:   decoded["sub"],
	}
	return user, nil
}

func SetUserContext(ctx context.Context, user *UserClaims) context.Context {
	return context.WithValue(ctx, "user", user)
}

func IsSessionValid(ctx context.Context) bool {
	user := ctx.Value("user")
	if user == nil {
		return false
	}
	_, ok := user.(*UserClaims)
	return ok
}
