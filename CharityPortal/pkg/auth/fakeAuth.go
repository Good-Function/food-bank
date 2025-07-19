package auth

import (
	"context"
	"net/url"
)

type FakeAuth struct {
}

func NewFakeAuth() (*FakeAuth, error) {
	return &FakeAuth{}, nil
}

func (a *FakeAuth) GetLoginURL() string {
	return "http://localhost"
}

func (a *FakeAuth) ExchangeToken(ctx context.Context, url url.Values) (*UserClaims, error) {
	return &UserClaims{
		Name:  "Test User",
		Email: "test@example.com",
		Sub:   "1234",
	}, nil
}

func (a *FakeAuth) Encode(name string, value interface{}) (string, error) {
	return "fake-encoded-cookie", nil
}

func (a *FakeAuth) Decode(name, value string) (*UserClaims, error) {
	return &UserClaims{
		Name:  "Test User",
		Email: "test@example.com",
		Sub:   "1234",
	}, nil
}
