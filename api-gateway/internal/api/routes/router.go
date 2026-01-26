package routes

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
	authRouter "api-gateway/internal/api/routes/auth"
	"api-gateway/internal/api/routes/calc"
	orderRouter "api-gateway/internal/api/routes/order"
	productRouter "api-gateway/internal/api/routes/product"

	"github.com/gin-gonic/gin"
)

type RouterGroup struct {
	Calc    calc.RouterGroup
	Auth    authRouter.RouterGroup
	Product productRouter.RouterGroup
	Order   orderRouter.RouterGroup
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

	calcGroup := RouterGroupApp.Calc
	authGroup := RouterGroupApp.Auth
	productGroup := RouterGroupApp.Product
	orderGroup := RouterGroupApp.Order

	MainGroup := r.Group("/api")
	{
		calcGroup.NewCalcRouter(cfg, MainGroup, h.CalcHandler)
		authGroup.NewAuthRouter(cfg, MainGroup, h.AuthHandler)
		productGroup.NewProductRouter(cfg, MainGroup, h.ProductHandler)
		orderGroup.NewOrderRouter(cfg, MainGroup, h.OrderHandler)
	}

	// --- NHÓM API SALES SERVICE ---
	// salesGroup := r.Group("/api/calc")
	// {
	// 	salesGroup.Any("/*proxyPath", proxy.NewReverseProxy(cfg.CalcServiceUrl))
	// }

	return r
}
