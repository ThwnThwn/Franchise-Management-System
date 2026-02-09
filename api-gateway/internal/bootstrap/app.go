package bootstrap

import (
	"api-gateway/config"
	"fmt"

	"github.com/gin-gonic/gin"
)

type App struct {
	Config *config.Config
	Server *gin.Engine
}

func NewApp(cfg *config.Config, server *gin.Engine) *App {
	return &App{
		Config: cfg,
		Server: server,
	}
}

func (a *App) Run() error {
	port := a.Config.Port
	fmt.Printf("Gateway is running on port %s\n", port)
	return a.Server.Run(":" + port)
}
