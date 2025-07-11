package middleware

import (
	"log/slog"
	"net/http"
	"time"
)

func Log(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		startProcessing := time.Now()
		h.ServeHTTP(w, r)
		took := time.Since(startProcessing).Microseconds()
		slog.With("METHOD", r.Method, "URL", r.URL.Path, "ms", took).Info("Request processed")
	})
}
