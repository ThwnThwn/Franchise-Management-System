package main

import (
	"api-gateway/config"
	"api-gateway/internal/di"
	"log"
)

func main() {
	// 1. Load Config
	cfg := config.LoadConfig()

	// 2. Init App
	app, err := di.InitializeApp(cfg)
	if err != nil {
		log.Fatalln("Init app error:", err)
	}

	// 3. Run
	if err := app.Run(); err != nil {
		log.Fatalln("Run error:", err)
	}
}
