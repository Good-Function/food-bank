package middlewares

import (
	"log/slog"
	"net/http"
	"time"
)

func WithLog(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		startProcessing := time.Now()
		h.ServeHTTP(w, r)
		took := time.Since(startProcessing).Milliseconds()
		slog.With("METHOD", r.Method, "URL", r.URL.Path, "ms", took).Debug("Request processed")
	})
}