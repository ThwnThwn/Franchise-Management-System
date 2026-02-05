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
	authMW *middleware.AuthMiddleware,
) {
	orderApi := r.Group("/orders")

	accessTokenSecret := cfg.JwtSecret
	privateApi := orderApi.Group("")
	privateApi.Use(authMW.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.GET("/me", orderHandler.GetMyOrders)
		privateApi.POST("", orderHandler.CreateOrder)

		privateApi.GET("/:id", orderHandler.GetOrderById)
		privateApi.PUT("/:id", orderHandler.UpdateOrder)
		privateApi.PATCH("/:id/status", orderHandler.UpdateOrderStatus)
		privateApi.POST("/:id/payment", orderHandler.CreatePaymentUrl)
	}
}
