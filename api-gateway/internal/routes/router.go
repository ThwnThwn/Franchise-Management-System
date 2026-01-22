package routes

import (
	"api-gateway/config"
	"api-gateway/internal/handlers"
	"api-gateway/internal/middleware"

	"github.com/gin-gonic/gin"
)

func SetupRouter(cfg *config.Config, calcHandler *handlers.CalcHandler) *gin.Engine {
	r := gin.Default()

	// Apply Global Middleware
	r.Use(middleware.CORSMiddleware())

	// Health check cho Gateway
	r.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{"status": "Gateway is running", "port": cfg.Port})
	})

	api := r.Group("/api")
	{
		calc := api.Group("/calc")
		{
			calc.POST("/add", calcHandler.AddNumbers)
		}

		// Sau này thêm product, sales vào đây:
		// product := api.Group("/products")
		// ...
	}

	// --- NHÓM API SALES SERVICE ---
	// salesGroup := r.Group("/api/calc")
	// {
	// 	salesGroup.Any("/*proxyPath", proxy.NewReverseProxy(cfg.CalcServiceUrl))
	// }

	return r
}
