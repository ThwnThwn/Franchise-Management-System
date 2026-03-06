package withdrawals

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (r *WithdrawalRouter) Init(router *gin.RouterGroup) {
	// ----------------------------------------------------------------------
	// NHÓM API DÀNH CHO ADMIN / KẾ TOÁN (Tiền tố: /withdrawals)
	// ----------------------------------------------------------------------
	adminWithdrawals := router.Group("/withdrawals")
	adminWithdrawals.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		// Xem danh sách và chi tiết (quyền read)
		adminWithdrawals.GET("", r.authMW.PermissionCheck("wallet.withdrawal.read"), api.WrapHandler(r.withdrawalHl.GetAllWithdrawals))
		adminWithdrawals.GET("/:id", r.authMW.PermissionCheck("wallet.withdrawal.read"), api.WrapHandler(r.withdrawalHl.GetWithdrawalById))

		// Duyệt và Từ chối (quyền approve/review)
		adminWithdrawals.PATCH("/:id/approve", r.authMW.PermissionCheck("wallet.withdrawal.approve"), api.WrapHandler(r.withdrawalHl.ApproveWithdrawal))
		adminWithdrawals.PATCH("/:id/reject", r.authMW.PermissionCheck("wallet.withdrawal.approve"), api.WrapHandler(r.withdrawalHl.RejectWithdrawal))

		// Hủy lệnh và Hoàn tất (quyền update/execute)
		// Admin Cancel: Hủy ở nhiều trạng thái khác nhau
		adminWithdrawals.PATCH("/:id/cancel", r.authMW.PermissionCheck("wallet.withdrawal.update"), api.WrapHandler(r.withdrawalHl.CancelWithdrawal))
		// Admin Complete: Chốt lệnh và upload ảnh bill
		adminWithdrawals.PATCH("/:id/complete", r.authMW.PermissionCheck("wallet.withdrawal.update"), api.WrapHandler(r.withdrawalHl.CompleteWithdrawal))
	}

	// ----------------------------------------------------------------------
	// NHÓM API DÀNH CHO END-USER (Tiền tố: /me/withdrawals)
	// ----------------------------------------------------------------------
	myWithdrawals := router.Group("/me/withdrawals")
	myWithdrawals.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		// Xem lịch sử rút tiền cá nhân
		myWithdrawals.GET("", r.authMW.PermissionCheck("wallet.withdrawal.read.me"), api.WrapHandler(r.withdrawalHl.GetMyWithdrawals))

		// Tạo lệnh rút tiền mới
		myWithdrawals.POST("", r.authMW.PermissionCheck("wallet.withdrawal.create.me"), api.WrapHandler(r.withdrawalHl.CreateWithdrawal))

		// Khách tự hủy lệnh (Chỉ khi còn Pending)
		myWithdrawals.PATCH("/:id/cancel", r.authMW.PermissionCheck("wallet.withdrawal.update.me"), api.WrapHandler(r.withdrawalHl.CancelMyWithdrawal))
	}
}
