package handlers

import (
	"charity_portal/web/layout"
	"net/http"
)

type HomeHandler struct{}

func NewHomeHandler() *HomeHandler {
	return &HomeHandler{}
}

func (ph *HomeHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	layout.Base(layout.Home(), "").Render(r.Context(), w)
}
