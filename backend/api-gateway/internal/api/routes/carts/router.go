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
		privateApi.GET("", c.authMW.PermissionCheck("cart.read"),
			api.WrapHandler(c.cartHl.GetMyCart))

		privateApi.POST("/items", c.authMW.PermissionCheck("cart.create"),
			api.WrapHandler(c.cartHl.AddToMyCart))

		privateApi.PUT("/items", c.authMW.PermissionCheck("cart.update"),
			api.WrapHandler(c.cartHl.UpdateMyCartItems))

		privateApi.DELETE("/items", c.authMW.PermissionCheck("cart.delete"),
			api.WrapHandler(c.cartHl.DeleteMyCartItems))

		privateApi.DELETE("", c.authMW.PermissionCheck("cart.delete"),
			api.WrapHandler(c.cartHl.ClearMyCart))
	}
}
