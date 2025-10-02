package middlewares

import (
	"charity_portal/internal/auth"
	"context"
	"net/http"
)


func BuildProtect(readSession func(r *http.Request) (*auth.SessionData, error)) func(http.HandlerFunc) http.HandlerFunc {
	return func(next http.HandlerFunc) http.HandlerFunc {
		return func(w http.ResponseWriter, r *http.Request) {
			data, err := readSession(r)
			if err != nil {
				http.Redirect(w, r, "/login", http.StatusFound)
				return
			}
			ctx := context.WithValue(r.Context(), auth.UserContextKey{}, data)
			next(w, r.WithContext(ctx))
		}
	}
}

func ProtectFake(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := int64(105)
		sessionData := &auth.SessionData{
			Email: "developeros@charity.pl",
			OrgID: &id,
		}
		ctx := context.WithValue(r.Context(), auth.UserContextKey{}, sessionData)
		next(w, r.WithContext(ctx))
	}
}
