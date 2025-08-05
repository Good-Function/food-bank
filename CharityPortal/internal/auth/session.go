package auth

import (
	"context"
	"crypto/rand"
	"encoding/base64"
	"errors"
	"net/http"
	"time"

	"github.com/gorilla/securecookie"
	"golang.org/x/oauth2"
)

type UserContextKey struct{}

type SessionManager struct {
	cookieCodec *securecookie.SecureCookie
	authConfig  *oauth2.Config
}

func NewSessionManager(secretKey, blockKey string, authConfig *oauth2.Config) *SessionManager {
	return &SessionManager{
		cookieCodec: securecookie.New([]byte(secretKey), nil),
		authConfig:  authConfig,
	}
}

func GenerateRandomState() string {
	b := make([]byte, 32)
	rand.Read(b)
	return base64.URLEncoding.EncodeToString(b)
}

func (s *SessionManager) ExchangeCodeForToken(ctx context.Context, code string) (*oauth2.Token, error) {
	return s.authConfig.Exchange(ctx, code)
}

func (s *SessionManager) WriteStateCookie(w http.ResponseWriter, state string) error {
	encoded, err := s.cookieCodec.Encode("oauth_state", state)
	if err != nil {
		return err
	}
	http.SetCookie(w, &http.Cookie{
		Name:     "oauth_state",
		Value:    encoded,
		Path:     "/",
		HttpOnly: true,
		Secure:   false,
	})
	return nil
}

func (s *SessionManager) ValidateState(r *http.Request) error {
	cookie, err := r.Cookie("oauth_state")
	if err != nil {
		return err
	}

	var expected string
	if err := s.cookieCodec.Decode("oauth_state", cookie.Value, &expected); err != nil {
		return err
	}

	if r.FormValue("state") != expected {
		return errors.New("state mismatch")
	}

	return nil
}

func (s *SessionManager) AuthProviderUrl(w http.ResponseWriter) string {
	state := GenerateRandomState()
	s.WriteStateCookie(w, state)
	return s.authConfig.AuthCodeURL(state)
}

func (s *SessionManager) WriteSession(w http.ResponseWriter, email string) error {
	encoded, err := s.cookieCodec.Encode("session", email)
	if err != nil {
		return err
	}

	http.SetCookie(w, &http.Cookie{
		Name:     "session",
		Value:    encoded,
		Path:     "/",
		HttpOnly: true,
		Secure:   true,
		SameSite: http.SameSiteLaxMode,
		Expires:  time.Now().Add(24 * time.Hour),
	})
	return nil
}

func (s *SessionManager) ReadSession(r *http.Request) (string, error) {
	cookie, err := r.Cookie("session")
	if err != nil {
		return "", err
	}

	var email string
	if err := s.cookieCodec.Decode("session", cookie.Value, &email); err != nil {
		return "", err
	}

	return email, nil
}
