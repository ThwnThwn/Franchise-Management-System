package orders

import (
	"github.com/gin-gonic/gin"
)

func (o *OrderRouter) Init(r *gin.RouterGroup) {
	orderApi := r.Group("/orders")

	privateApi := orderApi.Group("")
	privateApi.Use(o.authMW.ValidateToken([]byte(o.cfg.JwtSecret)))
	{
		privateApi.GET("/me", o.orderHl.GetMyOrders)
		privateApi.POST("", o.orderHl.CreateOrder)

		privateApi.GET("/:id", o.orderHl.GetOrderById)
		privateApi.PUT("/:id", o.orderHl.UpdateOrder)
		privateApi.PATCH("/:id/status", o.orderHl.UpdateOrderStatus)
		privateApi.POST("/:id/payment", o.orderHl.CreatePaymentUrl)
	}
}
