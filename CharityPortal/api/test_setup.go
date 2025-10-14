package api

import (
	"charity_portal/api/middlewares"
	"charity_portal/web"
	"fmt"
	"net/http"
)

func NewAuthenticatedRouter() http.Handler {
	readSession := func(r *http.Request) (*web.SessionData, error) {
		sessionData := web.SessionData{
			Email: "tester@jester.pl",
			OrgID: nil,
		}
		return &sessionData, nil
	}
	return newRouter(nil, nil, middlewares.BuildProtect(readSession), nil, nil)
}

func NewUnauthenticatedRouter() http.Handler {
	readSession := func(r *http.Request) (*web.SessionData, error) {
		return nil, fmt.Errorf("ała")
	}
	return newRouter(nil, nil, middlewares.BuildProtect(readSession), nil, nil)
}
