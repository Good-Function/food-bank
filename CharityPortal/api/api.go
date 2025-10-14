package api

import (
	"charity_portal/api/handlers"
	"charity_portal/api/middlewares"
	charity "charity_portal/charity_update"
	"charity_portal/charity_update/adapters"
	"charity_portal/charity_update/queries"
	"charity_portal/config"
	"charity_portal/internal/auth"
	dataconfirmation "charity_portal/internal/data_confirmation"
	"context"
	"fmt"
	"log"
	"log/slog"
	"net/http"
	"os"
	"strings"

	"github.com/coreos/go-oidc/v3/oidc"
	"golang.org/x/oauth2"
)

type API struct {
	server *http.Server
}

func configureAuth(cfg config.Auth) (*oauth2.Config, *oidc.IDTokenVerifier, error) {
	ctx := context.Background()
	providerURL := fmt.Sprintf("https://%s.ciamlogin.com/%s/v2.0", cfg.TenantID, cfg.TenantID)
	provider, err := oidc.NewProvider(ctx, providerURL)
	if err != nil {
		return nil, nil, fmt.Errorf("cannot create oidc provider; %w", err)
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
	return oauth2Config, verifier, nil
}

const environmentDevelopment = "Development"

func NewAPI(cfg *config.Config) (*API, error) {
	setupLogger(cfg.Logger)
	oauth2Config, tokenVerifier, err := configureAuth(*cfg.Auth)
	if err != nil {
		return nil, err
	}
	sessionManager := auth.NewSessionManager(cfg.Auth.HashKey, cfg.Auth.BlockKey, oauth2Config)
	var protect func(next http.HandlerFunc) http.HandlerFunc
	if cfg.Environment == environmentDevelopment {
		protect = middlewares.ProtectFake
	} else {
		protect = middlewares.BuildProtect(sessionManager.ReadSession)
	}
	var callOperator adapters.CallOperator
	if cfg.CharityUpdate.MockOperatorApi {
		callOperator = adapters.CallOperatorMock
	} else {
		callOperator = adapters.MakeCallOperator(cfg.CharityUpdate.OperatorApiClientId, cfg.CharityUpdate.OperatorApiBaseUrl)
	}
	charityRouter := protect(charity.CreateRouter(charity.Compose(*cfg.CharityUpdate)).ServeHTTP)
	router := newRouter(tokenVerifier, sessionManager, protect, &charityRouter, adapters.MakeReadOrganizationInfoByEmail(callOperator))
	server := http.Server{
		Addr:    ":8080",
		Handler: router,
	}
	slog.Info("started http://localhost:8080")
	return &API{
		server: &server,
	}, nil
}

func newRouter(
	tokenVerifier *oidc.IDTokenVerifier,
	sessionManager *auth.SessionManager,
	protect func(next http.HandlerFunc) http.HandlerFunc,
	charityRouter *http.HandlerFunc,
	readOrgInfo queries.ReadOrganizationIdByEmail,
) http.Handler {
	mux := http.NewServeMux()

	dataConfirmationService := dataconfirmation.NewDataConfirmationService()
	fs := http.FileServer(http.Dir("./web/static"))
	mux.Handle("/static/", http.StripPrefix("/static/", fs))
	mux.Handle("/",  protect(handlers.NewDashboardHandler().ServeHTTP)) // this
	mux.Handle("GET /login", handlers.LoginHandler(sessionManager))
	mux.Handle("GET /login/callback", handlers.NewLoginCallbackHandler(tokenVerifier, sessionManager, readOrgInfo))
	mux.Handle("GET /dashboard", protect(handlers.NewDashboardHandler().ServeHTTP)) // this
	mux.Handle("POST /logout", handlers.NewLogoutHandler())
	mux.Handle("POST /data-confirmation", protect(handlers.NewDataConfirmationHandler(dataConfirmationService).ServeHTTP))
	mux.Handle("/charity-update/", http.StripPrefix("/charity-update", charityRouter))
	notFoundWrapped := middlewares.WithNotFound(mux)
	forwardedWrapped := middlewares.WithForwardedHost(notFoundWrapped)
	return middlewares.WithLog(forwardedWrapped)
}

func (a *API) Start() {
	log.Fatalln(a.server.ListenAndServe())
}
func (a *API) Shutdown() {
	log.Fatalln(a.server.Close())
}

func setupLogger(cfg *config.Logger) {
	slogLogLevel := slog.LevelDebug
	switch strings.ToLower(cfg.Level) {
	case "debug":
		slogLogLevel = slog.LevelDebug
	case "info":
		slogLogLevel = slog.LevelInfo
	case "warn":
		slogLogLevel = slog.LevelWarn
	case "error":
		slogLogLevel = slog.LevelError
	default:
		log.Println("Invalid log level, defaulting to debug")
		slogLogLevel = slog.LevelDebug

	}
	logHandler := slog.NewJSONHandler(os.Stdout, &slog.HandlerOptions{
		Level: slogLogLevel,
	})
	slog.SetDefault(slog.New(logHandler))
}