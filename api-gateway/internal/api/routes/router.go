package routes

import (
	"api-gateway/config"
	"api-gateway/internal/api/middleware"
	"net/http"

	"github.com/gin-gonic/gin"
)

func NewEngine(cfg *config.Config, aR *AppRouter) *gin.Engine {
	r := gin.Default()
	// r.Use(gin.Recovery())

	// Apply Global Middleware
	r.Use(middleware.GlobalErrorHandler())
	r.Use(middleware.CORSMiddleware())

	// Health check cho Gateway
	r.GET("/health", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{
			"status": "UP",
			"port":   cfg.Port,
		})
	})

	MainGroup := r.Group("/api")
	{
		// Franchise
		aR.StoreRouter.Init(MainGroup)

		// Identity
		aR.AuthRouter.Init(MainGroup)
		aR.UserRouter.Init(MainGroup)
		aR.RoleRouter.Init(MainGroup)
		aR.PermissionRouter.Init(MainGroup)

		// Product
		aR.ProductRouter.Init(MainGroup)
		aR.CategoryRouter.Init(MainGroup)

		// Sales
		aR.OrderRouter.Init(MainGroup)
		aR.CartRouter.Init(MainGroup)
	}

	return r
}
