package middlewares

import (
	"charity_portal/internal/auth"
	"context"
	"net/http"
)

func BuildProtect(sessionManager *auth.SessionManager) func(http.HandlerFunc) http.HandlerFunc {
    return func(next http.HandlerFunc) http.HandlerFunc {
        return func(w http.ResponseWriter, r *http.Request) {
            email, err := sessionManager.ReadSession(r)
            if err != nil {
                http.Redirect(w, r, "/login", http.StatusFound)
                return
            }
            ctx := context.WithValue(r.Context(), auth.UserContextKey{}, email)
            next(w, r.WithContext(ctx))
        }
    }
}

func ProtectFake(next http.HandlerFunc) http.HandlerFunc {
    return func(w http.ResponseWriter, r *http.Request) {
        email := "developer@charity.pl"
        ctx := context.WithValue(r.Context(), auth.UserContextKey{}, email)
        next(w, r.WithContext(ctx))
    }
}