package main

import (
	"charity_portal/api"
	"charity_portal/config"
	"fmt"
)

func main() {
	cfg := config.LoadConfig()
	fmt.Println("Starting Charity Portal API in environment:", cfg.Environment)
	apiServer := api.NewAPI(cfg)
	apiServer.Start()
}
