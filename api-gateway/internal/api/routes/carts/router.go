package carts

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (c *CartRouter) Init(r *gin.RouterGroup) {
	myCartApi := r.Group("/me/cart")

	privateApi := myCartApi.Group("")
	privateApi.Use(c.authMW.ValidateToken([]byte(c.cfg.JwtSecret)))
	{
		privateApi.GET("", api.WrapHandler(c.cartHl.GetMyCart))
		privateApi.POST("/items", api.WrapHandler(c.cartHl.AddToMyCart))
		privateApi.PUT("/items", api.WrapHandler(c.cartHl.UpdateMyCartItems))
		privateApi.DELETE("/items", api.WrapHandler(c.cartHl.DeleteMyCartItems))
		privateApi.DELETE("", api.WrapHandler(c.cartHl.ClearMyCart))
	}
}
