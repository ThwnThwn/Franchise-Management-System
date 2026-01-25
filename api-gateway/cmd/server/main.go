package main

import (
	"api-gateway/config"
	"api-gateway/global"
	"api-gateway/internal/di"
	"log"
)

func main() {
	// 1. Load Config
	global.Config = config.LoadConfig()

	// 2. Init App
	app, err := di.InitializeApp(global.Config)
	if err != nil {
		log.Fatalln("Init app error:", err)
	}

	// 3. Run
	if err := app.Run(); err != nil {
		log.Fatalln("Run error:", err)
	}
}
