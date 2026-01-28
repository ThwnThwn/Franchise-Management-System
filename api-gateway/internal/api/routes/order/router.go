package order

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type OrderRouter struct{}

func (a *OrderRouter) NewOrderRouter(cfg *config.Config,
	r *gin.RouterGroup,
	orderHandler *handlers.OrderHandler,
) {
	orderApi := r.Group("/orders")
	{
		orderApi.GET("/:id", orderHandler.GetOrderById)
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := orderApi.Group("/")
	privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.GET("/me", orderHandler.GetMyOrders)
		privateApi.POST("/", orderHandler.CreateOrder)
	}
}
