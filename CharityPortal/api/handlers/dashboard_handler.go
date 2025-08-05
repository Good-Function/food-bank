package handlers

import (
	"charity_portal/internal/auth"
	"charity_portal/web/views"
	"net/http"
)

type DashboardHandler struct{}

func NewDashboardHandler() *DashboardHandler {
	return &DashboardHandler{}
}

func (ph *DashboardHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	email := r.Context().Value(auth.UserContextKey{}).(string)
	views.Base(views.Dashboard(), email).Render(r.Context(), w)
}
