package categories

import (
	"github.com/gin-gonic/gin"
)

func (c *CategoryRouter) Init(r *gin.RouterGroup) {
	categoryApi := r.Group("/categories")
	{
		categoryApi.GET("", c.categoryHl.GetCategories)
		categoryApi.GET("/:id", c.categoryHl.GetById)
	}

	privateApi := categoryApi.Group("")
	privateApi.Use(c.authMW.ValidateToken([]byte(c.cfg.JwtSecret)))
	{
		privateApi.POST("", c.authMW.PermissionCheck("category.create"), c.categoryHl.Create)
		privateApi.PATCH("/:id", c.authMW.PermissionCheck("category.update"), c.categoryHl.UpdateById)
		privateApi.DELETE("/:id", c.authMW.PermissionCheck("category.delete"), c.categoryHl.DeleteById)
	}
}
