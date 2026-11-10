package payments

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (r *SupplierPaymentRouter) Init(supplierR *gin.RouterGroup) {
	publicPayment := supplierR.Group("/payments")
	{
		publicPayment.GET("/vnpay/return", api.WrapHandler(r.paymentHl.HandleVnPayReturn))
	}

	paymentGroup := supplierR.Group("/payments")
	paymentGroup.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		paymentGroup.GET("", r.authMW.PermissionCheck("supplier.payment.read"), api.WrapHandler(r.paymentHl.GetAll))
		paymentGroup.POST("", r.authMW.PermissionCheck("supplier.payment.create"), api.WrapHandler(r.paymentHl.Create))
	}
}
