package routes

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

var RouterGroupApp = new(RouterGroup)

func NewRouter(cfg *config.Config, h *handlers.AppHandlers) *gin.Engine {
	r := gin.Default()
	r.Use(gin.Recovery())

	// Apply Global Middleware
	r.Use(middleware.GlobalErrorHandler())
	r.Use(middleware.CORSMiddleware())

	// Health check cho Gateway
	r.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{"status": "Gateway is running", "port": cfg.Port})
	})

	MainGroup := r.Group("/api")
	{
		// Franchise
		storeGroup.NewStoreRouter(cfg, MainGroup, h.StoreHandler)
		shiftGroup.NewShiftRouter(cfg, MainGroup, h.ShiftHandler)

		// Identity
		authGroup.NewAuthRouter(cfg, MainGroup, h.AuthHandler)
		userGroup.NewUserRouter(cfg, MainGroup, h.UserHandler, h.ShiftHandler)

		// Product
		productGroup.NewProductRouter(cfg, MainGroup, h.ProductHandler)
		categoryGroup.NewCategoryRouter(cfg, MainGroup, h.CategoryHandler)

		// Sales
		orderGroup.NewOrderRouter(cfg, MainGroup, h.OrderHandler)
		cartGroup.NewCartRouter(cfg, MainGroup, h.CartHandler)

		// Inventory
		inventoryGroup.NewInventoryRouter(cfg, MainGroup, h.InventoryHandler)
	}

	return r
}
