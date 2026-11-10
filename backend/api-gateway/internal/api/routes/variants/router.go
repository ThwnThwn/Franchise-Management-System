package variants

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (pV *VariantRouter) Init(r *gin.RouterGroup) {
	variantApi := r.Group("/variants")

	privateApi := variantApi.Group("")
	privateApi.Use(pV.authMW.ValidateToken([]byte(pV.cfg.JwtSecret)))
	{
		privateApi.PUT("/:id/recipes", pV.authMW.PermissionCheck("recipe.update"),
			api.WrapHandler(pV.recipeHl.MutationVariantRecipe))
	}
}
