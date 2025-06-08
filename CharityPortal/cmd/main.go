package main

import (
	"charity_portal/api"
	"charity_portal/config"
)

func main() {
	cfg := config.LoadConfig()
	apiServer := api.NewAPI(cfg)
	apiServer.Start()
}
