package categories

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (i *IngredientCategoryRouter) Init(ingreR *gin.RouterGroup) {
	ingreApi := ingreR.Group("/categories")

	privateApi := ingreApi.Group("")
	privateApi.Use(i.authMW.ValidateToken([]byte(i.cfg.JwtSecret)))
	{
		privateApi.GET("", i.authMW.PermissionCheck("ingredient-category.read"),
			api.WrapHandler(i.ingreCateHl.GetAll))

		privateApi.POST("", i.authMW.PermissionCheck("ingredient-category.create"),
			api.WrapHandler(i.ingreCateHl.Create))
		privateApi.PATCH("/:id", i.authMW.PermissionCheck("ingredient-category.update"),
			api.WrapHandler(i.ingreCateHl.UpdateById))
		privateApi.DELETE("/:id", i.authMW.PermissionCheck("ingredient-category.delete"),
			api.WrapHandler(i.ingreCateHl.DeleteById))
	}
}
