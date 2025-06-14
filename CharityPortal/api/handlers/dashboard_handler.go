
package handlers

import (
	"charity_portal/web/views"
	"net/http"
)

type DashboardHandler struct{}

func NewDashboardHandler() *HomeHandler {
	return &HomeHandler{}
}

func (ph *DashboardHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	views.Base(views.Dashboard()).Render(r.Context(), w)
}
