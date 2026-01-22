package server

import (
	"api-gateway/config"
	"api-gateway/internal/clients"
	"api-gateway/internal/handlers"
	"api-gateway/internal/routes"
	"fmt"
	"log"
)

func Run(cfg *config.Config) {
	calcClient, err := clients.NewCalcServiceClient(cfg.CalcServiceUrl)
	if err != nil {
		log.Fatalln("Failed to init Calc Client:", err)
	}

	calcHandler := handlers.NewCalcHandler(calcClient)

	r := routes.SetupRouter(cfg, calcHandler)

	fmt.Printf("--> API Gateway is running in port:%s\n", cfg.Port)
	fmt.Printf("--> Forwarding /api/calc -> %s\n", cfg.CalcServiceUrl)

	r.Run(":" + cfg.Port)
}
