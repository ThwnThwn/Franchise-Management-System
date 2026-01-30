package product

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
	"api-gateway/internal/domain/constants/roleslug"

	"github.com/gin-gonic/gin"
)

type ProductRouter struct{}

func (a *ProductRouter) NewProductRouter(cfg *config.Config,
	r *gin.RouterGroup,
	productHandler *handlers.ProductHandler,
) {
	productApi := r.Group("/products")
	{
		productApi.GET("", productHandler.GetProducts)
		productApi.GET("/:id", productHandler.GetProductById)
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := productApi.Group("/")
	privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))

	adminApi := privateApi.Group("")
	adminApi.Use(middleware.RequireRoles(string(roleslug.Admin)))
	{
		adminApi.POST("", productHandler.CreateProduct)
		adminApi.PUT("/:id", productHandler.UpdateProduct)
		adminApi.DELETE("/:id", productHandler.DeleteProduct)
	}
}
