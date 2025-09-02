package middlewares

import "net/http"

func WithForwardedHost(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if host := r.Header.Get("X-Forwarded-Host"); host != "" {
			r.Host = host
		}
		if proto := r.Header.Get("X-Forwarded-Proto"); proto != "" {
			r.URL.Scheme = proto
		}
		next.ServeHTTP(w, r)
	})
}
