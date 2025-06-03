package main

import (
	"charity_portal/api"
	"fmt"
	"log"
)

func main() {
	apiServer := api.NewAPI()
	fmt.Println("Starting")
	log.Printf("apiServer = %+v\n", apiServer)
	apiServer.Start()
}
