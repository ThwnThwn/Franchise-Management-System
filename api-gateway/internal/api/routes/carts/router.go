package carts

import (
	"github.com/gin-gonic/gin"
)

func (c *CartRouter) Init(r *gin.RouterGroup) {
	cartApi := r.Group("/carts")

	privateApi := cartApi.Group("")
	privateApi.Use(c.authMW.ValidateToken([]byte(c.cfg.JwtSecret)))
	{
		privateApi.GET("", c.cartHl.GetCartItems)
		privateApi.POST("", c.cartHl.AddToCart)
		privateApi.PUT("", c.cartHl.UpdateCartItem)
		privateApi.DELETE("", c.cartHl.DeleteListCartItem)
		privateApi.DELETE("/clear", c.cartHl.ClearCart)
	}
}
