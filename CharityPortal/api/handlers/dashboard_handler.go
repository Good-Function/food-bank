package handlers

import (
	"charity_portal/internal/auth"
	"charity_portal/web/layout"
	"fmt"
	"net/http"
)

type DashboardHandler struct{}

func NewDashboardHandler() *DashboardHandler {
	return &DashboardHandler{}
}

func (ph *DashboardHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	session := r.Context().Value(auth.UserContextKey{}).(*auth.SessionData)
	orgId := "None"
	if(session.OrgID != nil) {
		orgId = fmt.Sprintf("%d", *session.OrgID)
	}
	layout.Base(layout.Dashboard(), session.Email + orgId).Render(r.Context(), w)
}
