package purchaseorders

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (r *PurchaseOrderRouter) Init(supplierR *gin.RouterGroup) {
	// Group cha: /suppliers/purchase-orders
	poGroup := supplierR.Group("/purchase-orders")

	// Middleware xác thực token dùng chung cho group
	poGroup.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		// 1. Quản lý danh sách & Chi tiết
		poGroup.GET("", r.authMW.PermissionCheck("supplier.po.read"), api.WrapHandler(r.poHl.GetAll))
		poGroup.GET("/:id", r.authMW.PermissionCheck("supplier.po.read"), api.WrapHandler(r.poHl.GetById))
		poGroup.GET("/:id/payments", r.authMW.PermissionCheck("supplier.po.read"), api.WrapHandler(r.paymentHl.GetByPoId))

		// 2. Thao tác chính
		poGroup.POST("", r.authMW.PermissionCheck("supplier.po.create"), api.WrapHandler(r.poHl.Create))
		poGroup.PATCH("/:id", r.authMW.PermissionCheck("supplier.po.update"), api.WrapHandler(r.poHl.Update))

		// 3. Quy trình trạng thái (Hủy/Hoàn tất)
		poGroup.PATCH("/:id/cancel", r.authMW.PermissionCheck("supplier.po.update"), api.WrapHandler(r.poHl.Cancel))
		poGroup.PATCH("/:id/complete", r.authMW.PermissionCheck("supplier.po.complete"), api.WrapHandler(r.poHl.Complete))

		// 4. Utility: Lấy giá nguyên liệu mới nhất
		poGroup.POST("/latest-prices", r.authMW.PermissionCheck("supplier.po.read"), api.WrapHandler(r.poHl.GetLatestPrices))
	}
}
