package handlers

import (
	"fmt"
	"io"
	"net/http"
)

type PingHandler struct {

}

func NewPingHandler() *PingHandler {
	return &PingHandler{}
}

func (ph *PingHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	fmt.Println("got request")
	io.WriteString(w, "Hello world")
}
