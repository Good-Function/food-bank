package config

import "os"

type Config struct {
	AuthConfig *Auth
}

func LoadConfig() *Config {
	return &Config{
		AuthConfig: buildAuthConfig(),
	}
}

type Auth struct {
	ClientID     string
	ClientSecret string
	RedirectURL  string
	TenantName   string
	TenantID     string
	State        string
}

func buildAuthConfig() *Auth {
	return &Auth{
		ClientID:     os.Getenv("AUTH_CLIENT_ID"),
		ClientSecret: os.Getenv("AUTH_CLIENT_SECRET"),
		RedirectURL:  os.Getenv("AUTH_REDIRECT_URL"),
		TenantName:   os.Getenv("AUTH_TENANT_NAME"),
		TenantID:     os.Getenv("AUTH_TENANT_ID"),
		State:        os.Getenv("AUTH_STATE"),
	}
}
