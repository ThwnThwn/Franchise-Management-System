package products

import (
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

func (p *ProductRouter) Init(r *gin.RouterGroup) {
	productApi := r.Group("/products")
	{
		productApi.GET("", p.productHl.GetProducts)
		productApi.GET("/:id", p.productHl.GetProductById)
	}

	privateApi := productApi.Group("")
	privateApi.Use(p.authMW.ValidateToken([]byte(p.cfg.JwtSecret)))
	{
		privateApi.POST("", p.authMW.PermissionCheck("product.create"), p.productHl.CreateProduct)
		privateApi.PATCH("/:id", p.authMW.PermissionCheck("product.update"), p.productHl.UpdateProduct)
		privateApi.DELETE("/:id", p.authMW.PermissionCheck("product.delete"), p.productHl.DeleteProduct)

		privateApi.PUT("/:id/images",
			middleware.MaxUploadSize(p.cfg.MaxUploadSizeMb*1024*1024),
			p.productHl.UploadImage)
	}
}
