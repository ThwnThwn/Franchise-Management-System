package categories

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (c *CategoryRouter) Init(r *gin.RouterGroup) {
	categoryApi := r.Group("/categories")
	{
		categoryApi.GET("", api.WrapHandler(c.categoryHl.GetCategories))
		categoryApi.GET("/:id", api.WrapHandler(c.categoryHl.GetById))
	}

	privateApi := categoryApi.Group("")
	privateApi.Use(c.authMW.ValidateToken([]byte(c.cfg.JwtSecret)))
	{
		privateApi.POST("", c.authMW.PermissionCheck("category.create"), api.WrapHandler(c.categoryHl.Create))
		privateApi.PATCH("/:id", c.authMW.PermissionCheck("category.update"), api.WrapHandler(c.categoryHl.UpdateById))
		privateApi.DELETE("/:id", c.authMW.PermissionCheck("category.delete"), api.WrapHandler(c.categoryHl.DeleteById))
	}
}
