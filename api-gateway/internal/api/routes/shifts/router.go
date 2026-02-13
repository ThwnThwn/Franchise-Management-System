package shifts

import (
	"github.com/gin-gonic/gin"
)

func (s *ShiftsRouter) Init(r *gin.RouterGroup) {
	// Middleware xác thực Token dùng chung cho toàn bộ group
	auth := s.authMW.ValidateToken([]byte(s.cfg.JwtSecret))

	// ==========================================
	// NHÓM CA LÀM VIỆC (SHIFTS)
	// ==========================================
	shiftApi := r.Group("/shifts")
	shiftApi.Use(auth)
	{
		// 1. Lấy danh sách ca làm việc
		shiftApi.GET("", s.authMW.PermissionCheck("shift.read"), s.shiftHl.GetAll)

		// 2. Tạo ca làm mới
		shiftApi.POST("", s.authMW.PermissionCheck("shift.write"), s.shiftHl.Create)

		// 3. Lấy chi tiết một ca làm
		shiftApi.GET("/:id", s.authMW.PermissionCheck("shift.read"), s.shiftHl.GetByID)

		// 4. Cập nhật thông tin ca làm (Giờ giấc, số lượng nhân viên)
		shiftApi.PATCH("/:id", s.authMW.PermissionCheck("shift.write"), s.shiftHl.Update)

		// 5. Xóa ca làm
		shiftApi.DELETE("/:id", s.authMW.PermissionCheck("shift.delete"), s.shiftHl.Delete)

		// 6. Xuất bản ca làm lẻ
		shiftApi.PUT("/:id/publish", s.authMW.PermissionCheck("shift.publish"), s.shiftHl.Publish)

		// 7. Cập nhật phân công nhân sự (Assign users)
		shiftApi.PUT("/:id/assign", s.authMW.PermissionCheck("shift.write"), s.shiftHl.UpdateAssignments)

		// --- Thao tác hàng loạt (Bulk) ---
		// 1. Tạo ca thủ công hàng loạt
		shiftApi.POST("/bulk-manual", s.authMW.PermissionCheck("shift.write"), s.bulkHl.CreateManual)

		// 2. Tạo ca từ Template hàng loạt
		shiftApi.POST("/bulk-template", s.authMW.PermissionCheck("shift.write"), s.bulkHl.CreateFromTemplate)

		// 3. Công khai hàng loạt (Draft -> Published)
		shiftApi.PUT("/bulk-publish", s.authMW.PermissionCheck("shift.publish"), s.bulkHl.Publish)

		// 4. Xóa hàng loạt (Chỉ ca Draft)
		shiftApi.DELETE("/bulk-delete", s.authMW.PermissionCheck("shift.delete"), s.bulkHl.Delete)
	}

	// ==========================================
	// NHÓM MẪU CA LÀM (SHIFT TEMPLATES)
	// ==========================================
	templateApi := r.Group("/shift-templates")
	templateApi.Use(auth)
	{
		templateApi.GET("", s.authMW.PermissionCheck("shift.template.read"), s.templateHl.GetAll)
		templateApi.GET("/:id", s.authMW.PermissionCheck("shift.template.read"), s.templateHl.GetByID)
		templateApi.POST("", s.authMW.PermissionCheck("shift.template.write"), s.templateHl.Create)
		templateApi.PATCH("/:id", s.authMW.PermissionCheck("shift.template.write"), s.templateHl.Update)
		templateApi.DELETE("/:id", s.authMW.PermissionCheck("shift.template.delete"), s.templateHl.Delete)
	}

	// ==========================================
	// NHÓM QUY ĐỊNH CA LÀM (SHIFT RULES)
	// ==========================================
	ruleApi := r.Group("/shift-rules")
	ruleApi.Use(auth)
	{
		// Lấy cấu hình (ưu tiên theo storeId nếu có trong query)
		ruleApi.GET("", s.authMW.PermissionCheck("shift.rule.read"), s.ruleHl.GetSettings)

		// Lưu cấu hình (Dùng PUT để Update-or-Insert)
		ruleApi.PUT("", s.authMW.PermissionCheck("shift.rule.write"), s.ruleHl.Save)
	}

	// ==========================================
	// NHÓM LỊCH RẢNH/BẬN (AVAILABILITY)
	// ==========================================

	// --- GENERAL & MANAGER SIDE ---
	availabilityApi := r.Group("/shift-availabilities")
	availabilityApi.Use(auth)
	{
		// Manager lấy toàn bộ danh sách để kiểm soát
		availabilityApi.GET("", s.authMW.PermissionCheck("shift.availability.read_all"), s.availabilityHl.GetAll)

		// Staff tạo mới đơn báo rảnh/bận
		availabilityApi.POST("", s.availabilityHl.Create)

		// Staff cập nhật hoặc xóa đơn của mình (ID-based)
		availabilityApi.PATCH("/:id", s.availabilityHl.Update)
		availabilityApi.DELETE("/:id", s.availabilityHl.Delete)
	}

	// ==========================================
	// NHÓM ĐẶC BIỆT (STORES)
	// ==========================================
	storesApi := r.Group("/stores")
	storesApi.Use(auth)
	{
		storesApi.GET("/:storeId/shifts/:shiftId/candidates",
			s.authMW.PermissionCheck("shift.read"),
			s.shiftHl.GetAvailableStaffs,
		)
	}
}
