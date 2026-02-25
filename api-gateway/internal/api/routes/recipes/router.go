package recipes

import (
	"github.com/gin-gonic/gin"
)

func (rR *RecipeRouter) Init(r *gin.RouterGroup) {
	ingreApi := r.Group("/recipes")
	// ingreApi.Use(rR.authMW.ValidateToken([]byte(rR.cfg.JwtSecret)))
	{
		// ingreApi.GET("",
		// 	rR.authMW.PermissionCheck("recipe.read"), rR.recipeHl.GetAll)
		ingreApi.GET("", rR.recipeHl.GetAll)
	}
}
