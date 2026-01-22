package routes

import (
	"api-gateway/config"
	"api-gateway/internal/middleware"
	"api-gateway/internal/proxy"

	"github.com/gin-gonic/gin"
)

func SetupRouter(cfg *config.Config) *gin.Engine {
	r := gin.Default()

	// Apply Global Middleware
	r.Use(middleware.CORSMiddleware())

	// Health check cho Gateway
	r.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{"status": "Gateway is running", "port": cfg.Port})
	})

	// --- NHÓM API SALES SERVICE ---
	salesGroup := r.Group("/api/sales")
	{
		salesGroup.Any("/*proxyPath", proxy.NewReverseProxy(cfg.SalesService))
	}

	return r
}
