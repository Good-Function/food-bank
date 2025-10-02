package api

import (
	"charity_portal/api/middlewares"
	"charity_portal/internal/auth"
	"fmt"
	"net/http"
)

func NewAuthenticatedRouter() http.Handler {
	readSession := func(r *http.Request) (*auth.SessionData, error) {
		sessionData := auth.SessionData{
			Email: "tester@jester.pl",
			OrgID: nil,
		}
		return &sessionData, nil
	}
	return newRouter(nil, nil, middlewares.BuildProtect(readSession), nil)
}

func NewUnauthenticatedRouter() http.Handler {
	readSession := func(r *http.Request) (*auth.SessionData, error) {
		return nil, fmt.Errorf("ała")
	}
	return newRouter(nil, nil, middlewares.BuildProtect(readSession), nil)
}
