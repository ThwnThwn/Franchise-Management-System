package suppliers

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (r *SupplierRouter) Init(router *gin.RouterGroup) {
	suppliers := router.Group("/suppliers")

	privateApi := suppliers.Group("")
	privateApi.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		privateApi.GET("", r.authMW.PermissionCheck("supplier.read"), api.WrapHandler(r.supplierHl.GetAll))
		privateApi.POST("", r.authMW.PermissionCheck("supplier.create"), api.WrapHandler(r.supplierHl.Create))

		privateApi.GET("/:id", r.authMW.PermissionCheck("supplier.read"), api.WrapHandler(r.supplierHl.GetById))
		privateApi.PATCH("/:id", r.authMW.PermissionCheck("supplier.update"), api.WrapHandler(r.supplierHl.Update))
		privateApi.DELETE("/:id", r.authMW.PermissionCheck("supplier.delete"), api.WrapHandler(r.supplierHl.Delete))
	}

	r.poRouter.Init(suppliers)
	r.pmRouter.Init(suppliers)
}
