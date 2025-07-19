package mocks

import (
	"charity_portal/internal/auth"
	"context"
	"net/url"

	"github.com/stretchr/testify/mock"
)

type AuthProviderMock struct {
	mock.Mock
}

func (m *AuthProviderMock) GetLoginURL() string {
	args := m.Called()
	return args.String(0)
}

func (m *AuthProviderMock) ExchangeToken(ctx context.Context, url url.Values) (*auth.UserClaims, error) {
	args := m.Called(ctx, url)
	if claims, ok := args.Get(0).(*auth.UserClaims); ok {
		return claims, args.Error(1)
	}
	return nil, args.Error(1)
}

func (m *AuthProviderMock) Encode(name string, value interface{}) (string, error) {
	args := m.Called(name, value)
	return args.String(0), args.Error(1)
}
func (m *AuthProviderMock) Decode(name, value string) (*auth.UserClaims, error) {
	args := m.Called(name, value)
	if claims, ok := args.Get(0).(*auth.UserClaims); ok {
		return claims, args.Error(1)
	}
	return nil, args.Error(1)
}
