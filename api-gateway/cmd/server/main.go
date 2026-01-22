package main

import (
	"api-gateway/config"
	"api-gateway/internal/server"
)

func main() {
	// Load Config
	cfg := config.LoadConfig()

	server.Run(cfg)
}
