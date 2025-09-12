package config

import (
	charityupdate "charity_portal/charity_update"
	"fmt"
	"os"

	"github.com/BurntSushi/toml"
)

type Config struct {
	Auth        *Auth
	Logger      *Logger
	CharityUpdate *charityupdate.Config
	Environment string `toml:"environment"`
	OperatorDbConnectionString string `toml:"operator_db_connection_string"`
}

type Logger struct {
	Level string
}

type Auth struct {
	ClientID     string `toml:"client_id"`
	ClientSecret string `toml:"client_secret"`
	RedirectURL  string `toml:"redirect_url"`
	TenantID     string `toml:"tenant_id"`
	HashKey      string `toml:"hash_key"`
	BlockKey     string `toml:"block_key"`
}

func LoadConfig() *Config {
	configPath := "config.toml"
	var fileCfg Config
	_, err := toml.DecodeFile(configPath, &fileCfg)
	if err != nil {
		fmt.Println("Warning: could not read config.toml:", err)
	}
	overrideFromEnv(&fileCfg)
	return &fileCfg
}

func overrideFromEnv(config *Config) {
	config.Auth.ClientID = getEnv("AUTH_CLIENT_ID", config.Auth.ClientID)
	config.Auth.ClientSecret = getEnv("AUTH_CLIENT_SECRET", config.Auth.ClientSecret)
	config.Auth.RedirectURL = getEnv("AUTH_REDIRECT_URL", config.Auth.RedirectURL)
	config.Auth.TenantID = getEnv("AUTH_TENANT_ID", config.Auth.TenantID)
	config.Auth.HashKey = getEnv("AUTH_HASH_KEY", config.Auth.HashKey)
	config.Logger.Level = getEnv("LOG_LEVEL", config.Logger.Level)
	config.Environment = getEnv("APP_ENVIRONMENT", config.Environment)
	config.OperatorDbConnectionString = getEnv("OPERATOR_DB_CONNECTION_STRING", config.OperatorDbConnectionString)
	config.CharityUpdate.OperatorApiBaseUrl = getEnv("OPERATOR_API_BASE_URL", config.CharityUpdate.OperatorApiBaseUrl)
}

func getEnv(key, fallback string) string {
	if val := os.Getenv(key); val != "" {
		return val
	}
	return fallback
}
