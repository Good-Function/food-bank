package middlewares

import (
	"charity_portal/web"
	"context"
	"net/http"
)


func BuildProtect(readSession func(r *http.Request) (*web.SessionData, error)) func(http.HandlerFunc) http.HandlerFunc {
	return func(next http.HandlerFunc) http.HandlerFunc {
		return func(w http.ResponseWriter, r *http.Request) {
			data, err := readSession(r)
			if err != nil {
				http.Redirect(w, r, "/login", http.StatusFound)
				return
			}
			if data.OrgID == nil {
				http.Redirect(w, r, "/signup", http.StatusFound)
				return
			}
			ctx := context.WithValue(r.Context(), web.UserContextKey{}, data)
			next(w, r.WithContext(ctx))
		}
	}
}

func ProtectFake(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := int64(105)
		sessionData := &web.SessionData{
			Email: "developeros@charity.pl",
			OrgID: &id,
		}
		ctx := context.WithValue(r.Context(), web.UserContextKey{}, sessionData)
		next(w, r.WithContext(ctx))
	}
}
