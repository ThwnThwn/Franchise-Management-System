package main

import (
	"api-gateway/config"
	"api-gateway/global"
	"api-gateway/internal/di"
	"api-gateway/pkg/logger"

	"go.uber.org/zap"
)

// @title           API Gateway Documentation
// @version         1.0
// @description     API docs for CHD Gateway.
// @termsOfService  http://swagger.io/terms/

// @contact.name    API Support
// @contact.email   support@swagger.io

// @host            localhost:8080
// @BasePath        /api

// @securityDefinitions.apikey BearerAuth
// @in header
// @name Authorization
// @description Don't forget to type "Bearer" before the token. For example: Bearer eyJhbG...
func main() {
	// 1. Load Config
	global.Config = config.LoadConfig()
	l := logger.New(global.Config.Logger)

	// 2. Init App
	app, err := di.InitializeApp(global.Config, l)
	if err != nil {
		l.Fatal("Init app error:", zap.Error(err))
	}

	// 3. Run
	if err := app.Run(); err != nil {
		l.Fatal("Run error:", zap.Error(err))
	}
}
