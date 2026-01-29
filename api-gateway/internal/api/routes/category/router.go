package category

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type CategoryRouter struct{}

func (a *CategoryRouter) NewCategoryRouter(cfg *config.Config,
	r *gin.RouterGroup,
	categoryHander *handlers.CategoryHandler,
) {
	categoryApi := r.Group("/categories")
	{
		categoryApi.GET("", categoryHander.GetCategories)
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := categoryApi.Group("")
	privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))
	{
	}
}
