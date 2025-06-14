package handlers

import (
	"charity_portal/web/views"
	"net/http"
)

type HomeHandler struct{}

func NewHomeHandler() *HomeHandler {
	return &HomeHandler{}
}

func (ph *HomeHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	views.Base(views.Home()).Render(r.Context(), w)
}
