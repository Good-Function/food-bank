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

	"github.com/justinas/alice"
)

type API struct {
	server       *http.Server
	authProvider *auth.Auth
}

func NewAPI(cfg *config.Config) *API {
	logHandler := slog.NewJSONHandler(os.Stdout, &slog.HandlerOptions{
		Level: slog.LevelDebug,
	})
	slog.SetDefault(slog.New(logHandler))

	authProvider, err := auth.NewAuth(cfg.AuthConfig)
	if err != nil {
		log.Fatalf("Failed to create auth provider: %v", err)
	}

	router := newRouter(authProvider)

	server := http.Server{
		Addr:    ":8080",
		Handler: router,
	}

	return &API{
		server: &server,
	}
}

func newRouter(authProvider auth.AuthProvider) *http.ServeMux {
	mux := http.NewServeMux()

	commonMiddlewares := alice.New(middlewares.Log, middlewares.NewSessionMiddleware(authProvider).Session)
	notLoggedOnlyMiddlewares := commonMiddlewares.Extend(alice.New(middlewares.NewAuthMiddleware(authProvider).NotLogged))
	loggedOnlyMiddleware := commonMiddlewares.Extend(alice.New(middlewares.NewAuthMiddleware(authProvider).LoggedOnly))

	fs := http.FileServer(http.Dir("./web/static"))
	mux.Handle("GET /static/", commonMiddlewares.Then(http.StripPrefix("/static/", fs)))

	mux.Handle("GET /", notLoggedOnlyMiddlewares.Then(handlers.NewHomeHandler()))
	mux.Handle("POST /login", notLoggedOnlyMiddlewares.Then(handlers.NewLoginHandler(authProvider)))
	mux.Handle("GET /login/callback", notLoggedOnlyMiddlewares.Then(handlers.NewLoginCallbackHandler(authProvider)))

	mux.Handle("GET /dashboard", loggedOnlyMiddleware.Then(handlers.NewDashboardHandler()))
	return mux
}

func (a *API) Start() {
	log.Fatalln(a.server.ListenAndServe())
}
func (a *API) Shutdown() {
	log.Fatalln(a.server.Close())
}
