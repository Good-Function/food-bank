package middlewares

import "net/http"

/*
Middleware explanation:
- Azure Container Apps (ACA) sits behind a reverse proxy that terminates TLS and forwards requests to the container.
- Internally, the Go app sees requests as plain HTTP with Host=localhost, which can break URL generation and HTMX behavior.
- HTMX with `hx-push-url` relies on the browser seeing a matching origin and correct Location/response URLs.
- Without handling `X-Forwarded-Host` and `X-Forwarded-Proto`, HTMX may ignore push-url updates, causing the browser URL to remain constant.
- The WithForwardedHost middleware adjusts r.Host and r.URL.Scheme based on these headers so that:
    1. Redirects generate correct external URLs.
    2. HTMX push-url works properly even behind ACA.
- Wrapping the mux in WithForwardedHost before logging/not-found middleware ensures all handlers see the correct headers.
In F# we have the same, it's more ACA thing, not GO or F#.
*/

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
