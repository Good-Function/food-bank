package api

import (
	"charity_portal/api/handlers"
	"charity_portal/api/middlewares"
	"charity_portal/config"
	"charity_portal/pkg/auth"
	"log"
	"log/slog"
	"net/http"
	"os"
	"strings"

	"github.com/justinas/alice"
)

type API struct {
	server       *http.Server
	authProvider *auth.Auth
}

const (
	environmentDevelopment = "development"
	environmentProduction  = "production"
)

func NewAPI(cfg *config.Config) *API {
	setupLogger(cfg.Logger)
	appEnvironment := environmentProduction
	if strings.ToLower(cfg.Environment) == environmentDevelopment {
		appEnvironment = environmentDevelopment
	}

	authProvider, err := auth.NewAuth(cfg.AuthConfig)
	if err != nil {
		log.Fatalf("Failed to create auth provider: %v", err)
	}

	router := newRouter(authProvider, appEnvironment)

	server := http.Server{
		Addr:    ":8080",
		Handler: router,
	}

	return &API{
		server: &server,
	}
}

func newRouter(authProvider auth.AuthProvider, appEnvironment string) *http.ServeMux {
	mux := http.NewServeMux()

	commonMiddlewares := alice.New(middlewares.Log, middlewares.NewSessionMiddleware(authProvider, appEnvironment).Session)
	notLoggedOnlyMiddlewares := commonMiddlewares.Extend(alice.New(middlewares.NewAuthMiddleware(authProvider).NotLogged))
	loggedOnlyMiddleware := commonMiddlewares.Extend(alice.New(middlewares.NewAuthMiddleware(authProvider).LoggedOnly))

	fs := http.FileServer(http.Dir("./web/static"))
	mux.Handle("GET /static/", commonMiddlewares.Then(http.StripPrefix("/static/", fs)))

	mux.Handle("GET /", notLoggedOnlyMiddlewares.Then(handlers.NewHomeHandler()))
	mux.Handle("POST /login", notLoggedOnlyMiddlewares.Then(handlers.NewLoginHandler(authProvider)))
	mux.Handle("GET /login/callback", notLoggedOnlyMiddlewares.Then(handlers.NewLoginCallbackHandler(authProvider)))
	mux.Handle("POST /logout", loggedOnlyMiddleware.Then(handlers.NewLogoutHandler()))

	mux.Handle("GET /dashboard", loggedOnlyMiddleware.Then(handlers.NewDashboardHandler()))
	return mux
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
