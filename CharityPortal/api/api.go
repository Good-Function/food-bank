package api

import (
	"charity_portal/api/handlers"
	middleware "charity_portal/api/middlewares"
	"charity_portal/config"
	"charity_portal/pkg/auth"
	"log"
	"net/http"

	"github.com/justinas/alice"
)

type API struct {
	server       *http.Server
	authProvider *auth.Auth
}

func NewAPI(cfg *config.Config) *API {
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

	middlewaresChain := alice.New(middleware.Log)

	fs := http.FileServer(http.Dir("./web/static"))
	mux.Handle("GET /static/", middlewaresChain.Then(http.StripPrefix("/static/", fs)))

	mux.Handle("POST /login", middlewaresChain.Then(handlers.NewLoginHandler(authProvider)))
	mux.Handle("GET /login/callback", middlewaresChain.Then(handlers.NewLoginCallbackHandler(authProvider)))

	mux.Handle("GET /{$}", middlewaresChain.Then(handlers.NewHomeHandler()))
	return mux
}

func (a *API) Start() {
	log.Fatalln(a.server.ListenAndServe())
}
func (a *API) Shutdown() {
	log.Fatalln(a.server.Close())
}
