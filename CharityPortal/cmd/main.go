package main

import (
	"charity_portal/api"
)

func main() {
	apiServer := api.NewAPI()
	apiServer.Start()
}
