package recipes

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (rR *RecipeRouter) Init(r *gin.RouterGroup) {
	ingreApi := r.Group("/recipes")
	{
		ingreApi.GET("", api.WrapHandler(rR.recipeHl.GetAll))
	}
}
