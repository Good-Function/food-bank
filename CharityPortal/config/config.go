package config

import (
	"fmt"
	"os"
	"path/filepath"
	"runtime"

	"github.com/BurntSushi/toml"
)

type Config struct {
	Auth        *Auth
	Logger      *Logger
	Environment string `toml:"environment"`
}

type Logger struct {
	Level string
}

type Auth struct {
	ClientID     string `toml:"client_id"`
	ClientSecret string `toml:"client_secret"`
	RedirectURL  string `toml:"redirect_url"`
	TenantName   string `toml:"tenant_name"`
	TenantID     string `toml:"tenant_id"`
	State        string `toml:"state"`
	HashKey      string `toml:"hash_key"`
	BlockKey     string `toml:"block_key"`
}

func LoadConfig() *Config {
	_, filename, _, _ := runtime.Caller(0)
	root := filepath.Dir(filepath.Dir(filename))
	configPath := filepath.Join(root, "config.toml")

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
	config.Auth.TenantName = getEnv("AUTH_TENANT_NAME", config.Auth.TenantName)
	config.Auth.TenantID = getEnv("AUTH_TENANT_ID", config.Auth.TenantID)
	config.Auth.State = getEnv("AUTH_STATE", config.Auth.State)
	config.Auth.HashKey = getEnv("AUTH_HASH_KEY", config.Auth.HashKey)
	config.Auth.BlockKey = getEnv("AUTH_BLOCK_KEY", config.Auth.BlockKey)
	config.Logger.Level = getEnv("LOG_LEVEL", config.Logger.Level)
	config.Environment = getEnv("APP_ENVIRONMENT", config.Environment)
}

func getEnv(key, fallback string) string {
	if val := os.Getenv(key); val != "" {
		return val
	}
	return fallback
}
