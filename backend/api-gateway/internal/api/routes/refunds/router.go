package refunds

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (r *RefundRouter) Init(router *gin.RouterGroup) {
	// ----------------------------------------------------------------------
	// NHÓM API DÀNH CHO ADMIN / QUẢN LÝ (Tiền tố: /refunds)
	// ----------------------------------------------------------------------
	adminRefunds := router.Group("/refunds")
	adminRefunds.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		adminRefunds.GET("", r.authMW.PermissionCheck("wallet.refund.read"), api.WrapHandler(r.refundHl.GetAllRefunds))
		adminRefunds.GET("/:id", r.authMW.PermissionCheck("wallet.refund.read"), api.WrapHandler(r.refundHl.GetRefundById))

		// Cập nhật trạng thái
		adminRefunds.PATCH("/:id/approve", r.authMW.PermissionCheck("wallet.refund.approve"), api.WrapHandler(r.refundHl.ApproveRefund))
		adminRefunds.PATCH("/:id/reject", r.authMW.PermissionCheck("wallet.refund.approve"), api.WrapHandler(r.refundHl.RejectRefund))
		adminRefunds.PATCH("/:id/cancel", r.authMW.PermissionCheck("wallet.refund.update"), api.WrapHandler(r.refundHl.CancelRefund))
		adminRefunds.PATCH("/:id/complete", r.authMW.PermissionCheck("wallet.refund.update"), api.WrapHandler(r.refundHl.CompleteRefund))
	}

	// ----------------------------------------------------------------------
	// NHÓM API DÀNH CHO END-USER (Tiền tố: /me/refunds)
	// ----------------------------------------------------------------------
	myRefunds := router.Group("/me/refunds")
	myRefunds.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		myRefunds.GET("", r.authMW.PermissionCheck("wallet.refund.read.me"), api.WrapHandler(r.refundHl.GetMyRefunds))
		myRefunds.POST("", r.authMW.PermissionCheck("wallet.refund.create.me"), api.WrapHandler(r.refundHl.CreateRefund))
		// myRefunds.PATCH("/:id/cancel", r.authMW.PermissionCheck("wallet.refund.update.me"), api.WrapHandler(r.refundHl.CancelRefund))
	}
}
