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
	user := r.Context().Value(auth.UserContextKey{})
    if user == nil {
        http.Error(w, "User not in context", http.StatusUnauthorized)
        return
    }

    email := user.(string)
	views.Base(views.Dashboard(), email).Render(r.Context(), w)
}
