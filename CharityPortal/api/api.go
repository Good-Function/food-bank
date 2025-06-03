package api

import (
	"charity_portal/api/handlers"
	"log"
	"net/http"
)

type API struct {
	server *http.Server
}

func NewAPI() *API {
	router := http.NewServeMux()
	registerRoutes(router)

	server := http.Server{
		Addr:    ":8080",
		Handler: router,
	}

	return &API{
		server: &server,
	}
}

func registerRoutes(mux *http.ServeMux) {
	mux.Handle("GET /ping", handlers.NewPingHandler())
}

func (a *API) Start() {
	log.Fatalln(a.server.ListenAndServe())
}
func (a *API) Shutdown() {
	log.Fatalln(a.server.Close())
}
