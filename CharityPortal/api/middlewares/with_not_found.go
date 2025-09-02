package middlewares

import (
	"net/http"
)

func WithNotFound(mux *http.ServeMux) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		h, pattern := mux.Handler(r)
		if pattern == "/" && r.URL.Path != "/" {
			http.NotFound(w, r)
			return
		}
		h.ServeHTTP(w, r)
	})
}

