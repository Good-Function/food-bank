package handlers

import (
	"charity_portal/api/views"
	"charity_portal/web/layout"
	"net/http"
)

func SignupHandler() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		layout.Base(views.Signup(), "").Render(r.Context(), w)
	}
}
