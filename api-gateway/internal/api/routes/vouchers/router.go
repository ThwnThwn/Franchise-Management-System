package vouchers

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (v *VoucherRouter) Init(r *gin.RouterGroup) {
	// Group chính: /vouchers
	voucherApi := r.Group("/vouchers")
	{
		// GET /vouchers
		voucherApi.GET("", api.WrapHandler(v.voucherHl.GetAllVouchers))
		// GET /vouchers/code/:code
		voucherApi.GET("/code/:code", api.WrapHandler(v.voucherHl.GetVoucherByCode))
	}

	// PRIVATE: Các thao tác quản trị yêu cầu Login & Quyền hạn
	privateApi := voucherApi.Group("")
	privateApi.Use(v.authMW.ValidateToken([]byte(v.cfg.JwtSecret)))
	{
		// POST /vouchers -> Tạo mới
		privateApi.POST("", v.authMW.PermissionCheck("voucher.create"),
			api.WrapHandler(v.voucherHl.CreateVoucher))

		// PATCH /vouchers/:id -> Cập nhật thông tin
		privateApi.PATCH("/:id", v.authMW.PermissionCheck("voucher.update"),
			api.WrapHandler(v.voucherHl.UpdateVoucher))

		// PATCH /vouchers/:id/status -> Cập nhật trạng thái On/Off
		privateApi.PATCH("/:id/status", v.authMW.PermissionCheck("voucher.update"),
			api.WrapHandler(v.voucherHl.UpdateVoucherStatus))

		// DELETE /vouchers/:id -> Xóa voucher
		privateApi.DELETE("/:id", v.authMW.PermissionCheck("voucher.delete"),
			api.WrapHandler(v.voucherHl.DeleteVoucher))
	}
}
