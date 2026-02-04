package product

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type ProductRouter struct{}

func (a *ProductRouter) NewProductRouter(cfg *config.Config,
	r *gin.RouterGroup,
	productHandler *handlers.ProductHandler,
	authMW *middleware.AuthMiddleware,
) {
	productApi := r.Group("/products")
	{
		productApi.GET("", productHandler.GetProducts)
		productApi.GET("/:id", productHandler.GetProductById)
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := productApi.Group("")
	privateApi.Use(authMW.ValidateToken([]byte(accessTokenSecret)))

	// adminApi := privateApi.Group("")
	// adminApi.Use(middleware.RequireRoles(string(roleslug.Admin)))
	{
		privateApi.POST("", authMW.PermissionCheck("product.create"), productHandler.CreateProduct)
		privateApi.PATCH("/:id", authMW.PermissionCheck("product.update"), productHandler.UpdateProduct)
		privateApi.DELETE("/:id", authMW.PermissionCheck("product.delete"), productHandler.DeleteProduct)
	}
}
