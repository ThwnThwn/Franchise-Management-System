package products

import (
	"api-gateway/internal/api"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

func (p *ProductRouter) Init(r *gin.RouterGroup) {
	productApi := r.Group("/products")
	{
		productApi.GET("", api.WrapHandler(p.productHl.GetProducts))
		productApi.GET("/variants", api.WrapHandler(p.productHl.GetProductVariants))

		productApi.GET("/:id", api.WrapHandler(p.productHl.GetProductById))
	}

	privateApi := productApi.Group("")
	privateApi.Use(p.authMW.ValidateToken([]byte(p.cfg.JwtSecret)))
	{
		privateApi.POST("", p.authMW.PermissionCheck("product.create"),
			api.WrapHandler(p.productHl.CreateProduct))
		privateApi.PATCH("/:id", p.authMW.PermissionCheck("product.update"),
			api.WrapHandler(p.productHl.UpdateProduct))
		privateApi.DELETE("/:id", p.authMW.PermissionCheck("product.delete"),
			api.WrapHandler(p.productHl.DeleteProduct))

		privateApi.PUT("/:id/images",
			middleware.MaxUploadSize(p.cfg.MaxUploadSizeMb*1024*1024),
			api.WrapHandler(p.productHl.UploadImage))
	}
}
