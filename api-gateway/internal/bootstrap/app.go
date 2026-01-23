package bootstrap

import (
	"api-gateway/config"
	"fmt"

	"github.com/gin-gonic/gin"
)

type App struct {
	Server *gin.Engine
	Config *config.Config
}

func NewApp(cfg *config.Config, server *gin.Engine) *App {
	return &App{
		Server: server,
		Config: cfg,
	}
}

func (a *App) Run() error {
	port := a.Config.Port
	fmt.Printf("🚀 Gateway is running on port %s\n", port)
	return a.Server.Run(":" + port)
}
