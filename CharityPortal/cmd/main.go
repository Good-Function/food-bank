package main

import (
	"charity_portal/api"
	"charity_portal/config"
	"fmt"
)

func main() {
	cfg := config.LoadConfig()
	fmt.Println("Starting Charity Portal API in environment:", cfg.Environment)
	apiServer, err := api.NewAPI(cfg)
	if err != nil {
		fmt.Println("Error starting API server:", err)
		return
	}
	apiServer.Start()
}
