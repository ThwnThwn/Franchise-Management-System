package server

import (
	"api-gateway/config"
	"api-gateway/internal/routes"
	"fmt"
)

func Run(cfg *config.Config) {
	r := routes.SetupRouter(cfg)

	fmt.Printf("--> API Gateway is running in port:%s\n", cfg.Port)
	fmt.Printf("--> Forwarding /api/sales -> %s\n", cfg.SalesService)

	r.Run(":" + cfg.Port)
}
