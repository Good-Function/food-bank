package handlers

import (
	"charity_portal/web/views"
	"net/http"
)

type DashboardHandler struct{}

func NewDashboardHandler() *DashboardHandler {
	return &DashboardHandler{}
}

func (ph *DashboardHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	views.Base(views.Dashboard()).Render(r.Context(), w)
}
