package config

import "os"

type Config struct {
	AuthConfig *Auth
	Logger     *Logger
}

func LoadConfig() *Config {
	return &Config{
		AuthConfig: buildAuthConfig(),
		Logger:     buildLogger(),
	}
}

type Logger struct {
	Level string
}

func buildLogger() *Logger {
	return &Logger{
		Level: os.Getenv("LOG_LEVEL"),
	}
}

type Auth struct {
	ClientID     string
	ClientSecret string
	RedirectURL  string
	TenantName   string
	TenantID     string
	State        string
	HashKey      string
	BlockKey     string
}

func buildAuthConfig() *Auth {
	return &Auth{
		ClientID:     os.Getenv("AUTH_CLIENT_ID"),
		ClientSecret: os.Getenv("AUTH_CLIENT_SECRET"),
		RedirectURL:  os.Getenv("AUTH_REDIRECT_URL"),
		TenantName:   os.Getenv("AUTH_TENANT_NAME"),
		TenantID:     os.Getenv("AUTH_TENANT_ID"),
		State:        os.Getenv("AUTH_STATE"),
		HashKey:      os.Getenv("AUTH_HASH_KEY"),
		BlockKey:     os.Getenv("AUTH_BLOCK_KEY"),
	}
}
