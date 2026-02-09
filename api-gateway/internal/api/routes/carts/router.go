package carts

import (
	"github.com/gin-gonic/gin"
)

func (c *CartRouter) Init(r *gin.RouterGroup) {
	myCartApi := r.Group("/me/cart")

	privateApi := myCartApi.Group("")
	privateApi.Use(c.authMW.ValidateToken([]byte(c.cfg.JwtSecret)))
	{
		privateApi.GET("", c.cartHl.GetMyCart)
		privateApi.POST("/items", c.cartHl.AddToMyCart)
		privateApi.PUT("/items", c.cartHl.UpdateMyCartItems)
		privateApi.DELETE("/items", c.cartHl.DeleteMyCartItems)
		privateApi.DELETE("", c.cartHl.ClearMyCart)
	}
}
