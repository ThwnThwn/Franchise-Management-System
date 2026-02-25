package orders

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (o *OrderRouter) Init(r *gin.RouterGroup) {
	orderApi := r.Group("/orders")

	privateApi := orderApi.Group("")
	privateApi.Use(o.authMW.ValidateToken([]byte(o.cfg.JwtSecret)))
	{
		privateApi.GET("", api.WrapHandler(o.orderHl.GetOrders))
		privateApi.POST("", o.orderHl.CreateOrder)

		privateApi.GET("/:id", api.WrapHandler(o.orderHl.GetOrderById))
		privateApi.PUT("/:id", o.orderHl.UpdateOrder)
		privateApi.PATCH("/:id/status", o.orderHl.UpdateOrderStatus)
		privateApi.POST("/:id/payment", o.orderHl.CreatePaymentUrl)
	}

	myOrderApi := r.Group("/me/orders")
	myOrderApi.Use(o.authMW.ValidateToken([]byte(o.cfg.JwtSecret)))
	{
		myOrderApi.GET("", api.WrapHandler(o.orderHl.GetMyOrders))
	}
}
