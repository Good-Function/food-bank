package api

import (
	"charity_portal/api/middlewares"
	"fmt"
	"net/http"
)

func NewAuthenticatedRouter() http.Handler {
	readSession := func(r *http.Request) (string, error) {
		return "tester@jester.com", nil
	}
	return newRouter(nil, nil, middlewares.BuildProtect(readSession), nil)
}

func NewUnauthenticatedRouter() http.Handler {
	readSession := func(r *http.Request) (string, error) {
		return "", fmt.Errorf("ała!")
	}
	return newRouter(nil, nil, middlewares.BuildProtect(readSession), nil)
}
