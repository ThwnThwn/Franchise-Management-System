package routes

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
	"api-gateway/internal/api/routes/auth"
	"api-gateway/internal/api/routes/category"
	"api-gateway/internal/api/routes/order"
	"api-gateway/internal/api/routes/product"
	"api-gateway/internal/api/routes/user"

	"github.com/gin-gonic/gin"
)

type RouterGroup struct {
	Auth     auth.RouterGroup
	User     user.RouterGroup
	Product  product.RouterGroup
	Category category.RouterGroup
	Order    order.RouterGroup
}

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

	authGroup := RouterGroupApp.Auth
	userGroup := RouterGroupApp.User

	productGroup := RouterGroupApp.Product
	categoryGroup := RouterGroupApp.Category

	orderGroup := RouterGroupApp.Order

	MainGroup := r.Group("/api")
	{
		authGroup.NewAuthRouter(cfg, MainGroup, h.AuthHandler)
		userGroup.NewUserRouter(cfg, MainGroup, h.UserHandler)

		productGroup.NewProductRouter(cfg, MainGroup, h.ProductHandler)
		categoryGroup.NewCategoryRouter(cfg, MainGroup, h.CategoryHandler)

		orderGroup.NewOrderRouter(cfg, MainGroup, h.OrderHandler)
	}

	return r
}
