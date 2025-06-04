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
	fs := http.FileServer(http.Dir("./web/static"))
	mux.Handle("GET /static/", http.StripPrefix("/static/", fs))

	mux.Handle("GET /{$}", handlers.NewHomeHandler())
}

func (a *API) Start() {
	log.Fatalln(a.server.ListenAndServe())
}
func (a *API) Shutdown() {
	log.Fatalln(a.server.Close())
}
