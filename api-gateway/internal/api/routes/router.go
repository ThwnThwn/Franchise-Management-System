package routes

import (
	"api-gateway/config"
	"api-gateway/internal/api/middleware"
	"net/http"

	_ "api-gateway/docs"

	"github.com/gin-gonic/gin"
	swaggerFiles "github.com/swaggo/files"
	ginSwagger "github.com/swaggo/gin-swagger"
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

	// Truy cập: http://localhost:8080/swagger/index.html
	r.GET("/swagger/*any", ginSwagger.WrapHandler(swaggerFiles.Handler))

	MainGroup := r.Group("/api")
	{
		// Franchise
		aR.StoreRouter.Init(MainGroup)
		aR.MyShiftsRouter.Init(MainGroup)

		// Identity
		aR.AuthRouter.Init(MainGroup)
		aR.UserRouter.Init(MainGroup)
		aR.RoleRouter.Init(MainGroup)
		aR.PermissionRouter.Init(MainGroup)

		// Inventory
		aR.IngredientRouter.Init(MainGroup)
		aR.RecipeRouter.Init(MainGroup)

		// Product
		aR.ProductRouter.Init(MainGroup)
		aR.CategoryRouter.Init(MainGroup)
		aR.VariantRouter.Init(MainGroup)

		// Sales
		aR.OrderRouter.Init(MainGroup)
		aR.CartRouter.Init(MainGroup)
	}

	return r
}
