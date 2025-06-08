package api

import (
	"charity_portal/api/handlers"
	"charity_portal/config"
	"charity_portal/pkg/auth"
	"log"
	"net/http"
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

	fs := http.FileServer(http.Dir("./web/static"))
	mux.Handle("GET /static/", http.StripPrefix("/static/", fs))

	mux.Handle("POST /login", handlers.NewLoginHandler(authProvider))
	mux.Handle("GET /login/callback", handlers.NewLoginCallbackHandler(authProvider))

	mux.Handle("GET /{$}", handlers.NewHomeHandler())
	return mux
}

func (a *API) Start() {
	log.Fatalln(a.server.ListenAndServe())
}
func (a *API) Shutdown() {
	log.Fatalln(a.server.Close())
}
