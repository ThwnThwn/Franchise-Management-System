package ingredients

import (
	"github.com/gin-gonic/gin"
)

func (i *IngredientRouter) Init(r *gin.RouterGroup) {
	ingreApi := r.Group("/ingredients")

	privateApi := ingreApi.Group("")
	privateApi.Use(i.authMW.ValidateToken([]byte(i.cfg.JwtSecret)))
	{
		privateApi.GET("",
			i.authMW.PermissionCheck("ingredient.read"), i.ingredientHl.GetAll)

		privateApi.POST("", i.authMW.PermissionCheck("ingredient.create"), i.ingredientHl.Create)
		privateApi.PATCH("/:id", i.authMW.PermissionCheck("ingredient.update"), i.ingredientHl.UpdateById)
		privateApi.DELETE("/:id", i.authMW.PermissionCheck("ingredient.delete"), i.ingredientHl.DeleteById)
	}

	i.ingreCateR.Init(ingreApi)
}
