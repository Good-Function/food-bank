package handlers

import (
	"charity_portal/internal/user"
	"charity_portal/pkg/auth"
	"charity_portal/web/views"
	"net/http"
)

type DashboardHandler struct{}

func NewDashboardHandler() *DashboardHandler {
	return &DashboardHandler{}
}

func (ph *DashboardHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	userClaims := auth.GetUserFromContext(r.Context())
	userData := &user.UserData{
		ID:    userClaims.Sub,
		Email: userClaims.Email,
	}
	views.Base(views.Dashboard(), userData).Render(r.Context(), w)
}
