package api

import (
	"charity_portal/api/handlers"
	"charity_portal/api/middlewares"
	"charity_portal/config"
	"charity_portal/internal/auth"
	dataconfirmation "charity_portal/internal/data_confirmation"
	"log"
	"log/slog"
	"net/http"
	"os"
	"strings"

	"github.com/justinas/alice"
)

type API struct {
	server *http.Server
}

const (
	environmentDevelopment = "development"
	environmentProduction  = "production"
)

func NewAPI(cfg *config.Config) (*API, error) {
	setupLogger(cfg.Logger)
	authProvider, err := setupAuthProvider(cfg, cfg.Environment)
	if err != nil {
		return nil, err
	}
	router := newRouter(authProvider)
	server := http.Server{
		Addr:    ":8080",
		Handler: router,
	}
	slog.Info("started http://localhost:8080")
	return &API{
		server: &server,
	}, nil
}

func newRouter(authProvider auth.AuthProvider) http.Handler {
	mux := http.NewServeMux()

	dataConfirmationService := dataconfirmation.NewDataConfirmationService()
	protected := alice.New(middlewares.NewAuth2Middleware(authProvider).Protect)

	fs := http.FileServer(http.Dir("./web/static"))
	mux.Handle("GET /static/", http.StripPrefix("/static/", fs))

	mux.Handle("GET /", handlers.NewHomeHandler())
	mux.Handle("POST /login", handlers.NewLoginHandler(authProvider))
	mux.Handle("GET /login/callback", handlers.NewLoginCallbackHandler(authProvider))
	mux.Handle("POST /logout", protected.Then(handlers.NewLogoutHandler()))

	mux.Handle("GET /dashboard", handlers.NewDashboardHandler())
	mux.Handle("POST /data-confirmation", protected.Then(handlers.NewDataConfirmationHandler(dataConfirmationService)))
	return middlewares.Log(mux)
}

func (a *API) Start() {
	log.Fatalln(a.server.ListenAndServe())
}
func (a *API) Shutdown() {
	log.Fatalln(a.server.Close())
}

func setupAuthProvider(cfg *config.Config, env string) (auth.AuthProvider, error) {
	if env == environmentDevelopment {
		return auth.NewFakeAuth()
	}
	return auth.NewAuth(cfg.Auth)
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
