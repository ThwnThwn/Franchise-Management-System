package shifts

import (
	"api-gateway/internal/api"

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
		// Lấy danh sách ca làm việc
		shiftApi.GET("", s.authMW.PermissionCheck("shift.read"),
			api.WrapHandler(s.shiftHl.GetAll))

		// Tạo ca làm mới
		shiftApi.POST("", s.authMW.PermissionCheck("shift.write"),
			api.WrapHandler(s.shiftHl.Create))

		shiftApi.PUT("/auto-fill", s.authMW.PermissionCheck("shift.write"),
			api.WrapHandler(s.shiftHl.AutoFill))

		// Lấy chi tiết một ca làm
		shiftApi.GET("/:id", s.authMW.PermissionCheck("shift.read"),
			api.WrapHandler(s.shiftHl.GetByID))

		// Cập nhật thông tin ca làm (Giờ giấc, số lượng nhân viên)
		shiftApi.PATCH("/:id", s.authMW.PermissionCheck("shift.write"),
			api.WrapHandler(s.shiftHl.Update))

		// Xóa ca làm
		shiftApi.DELETE("/:id", s.authMW.PermissionCheck("shift.delete"),
			api.WrapHandler(s.shiftHl.Delete))

		// Xuất bản ca làm lẻ
		shiftApi.PUT("/:id/publish", s.authMW.PermissionCheck("shift.publish"),
			api.WrapHandler(s.shiftHl.Publish))

	}

	// --- Phân công nhân sự ---
	{
		shiftApi.PUT("/:id/assign", s.authMW.PermissionCheck("shift.write"),
			api.WrapHandler(s.shiftHl.UpdateAssignments))

		shiftApi.POST("/:id/applications",
			s.authMW.PermissionCheck("shift.application.write"),
			api.WrapHandler(s.appHl.Apply))

		shiftApi.GET("/:id/applications",
			s.authMW.PermissionCheck("shift.application.read"),
			api.WrapHandler(s.appHl.GetByShiftId))
	}

	// --- Thao tác hàng loạt (Bulk) ---
	{
		// Tạo ca thủ công hàng loạt
		shiftApi.POST("/bulk-manual", s.authMW.PermissionCheck("shift.write"),
			api.WrapHandler(s.bulkHl.CreateManual))

		// Tạo ca từ Template hàng loạt
		shiftApi.POST("/bulk-template", s.authMW.PermissionCheck("shift.write"),
			api.WrapHandler(s.bulkHl.CreateFromTemplate))

		// Công khai hàng loạt (Draft -> Published)
		shiftApi.PUT("/bulk-publish", s.authMW.PermissionCheck("shift.publish"),
			api.WrapHandler(s.bulkHl.Publish))

		// Xóa hàng loạt (Chỉ ca Draft)
		shiftApi.DELETE("/bulk-delete", s.authMW.PermissionCheck("shift.delete"),
			api.WrapHandler(s.bulkHl.Delete))

		shiftApi.PUT("/bulk-open", s.authMW.PermissionCheck("shift.delete"),
			api.WrapHandler(s.bulkHl.Open))
	}

	// --- Swap ---
	{
		shiftApi.POST("/:id/swaps",
			s.authMW.PermissionCheck("shift.swap.write"),
			api.WrapHandler(s.swapHl.RequestSwap))

		shiftApi.PATCH("/:id/releases",
			s.authMW.PermissionCheck("shift.release.write"),
			api.WrapHandler(s.swapHl.Release))
	}
	swapApi := r.Group("/shift-swaps")
	swapApi.Use(auth)
	{
		// Phản hồi từ đồng nghiệp (B phản hồi cho A)
		swapApi.PATCH("/:id/responses",
			s.authMW.PermissionCheck("shift.swap.write"),
			api.WrapHandler(s.swapHl.RespondSwap))

		// Manager duyệt cuối cùng
		swapApi.PATCH("/:id/approvals",
			s.authMW.PermissionCheck("shift.swap.approve"),
			api.WrapHandler(s.swapHl.ApproveSwap))

		// Hủy yêu cầu
		swapApi.PATCH("/:id/cancel",
			s.authMW.PermissionCheck("shift.swap.cancel"),
			api.WrapHandler(s.swapHl.CancelSwap))
	}

	// ==========================================
	// NHÓM NHẬT KÝ CHẤM CÔNG (TIME LOGS)
	// ==========================================
	timeLogApi := shiftApi.Group("/time-logs")
	{
		// 1. Lấy toàn bộ nhật ký chấm công (kèm bộ lọc: store, staff, status, date)
		timeLogApi.GET("",
			s.authMW.PermissionCheck("shift.time_log.read"),
			api.WrapHandler(s.shiftHl.GetAllTimeLogs),
		)

		// 2. Duyệt hoặc cập nhật trạng thái chấm công (Valid/Invalid + Note)
		timeLogApi.PATCH("/:id",
			s.authMW.PermissionCheck("shift.time_log.write"),
			api.WrapHandler(s.shiftHl.UpdateTimeLog),
		)
	}

	// ==========================================
	// NHÓM MẪU CA LÀM (SHIFT TEMPLATES)
	// ==========================================
	templateApi := r.Group("/shift-templates")
	templateApi.Use(auth)
	{
		templateApi.GET("", s.authMW.PermissionCheck("shift.template.read"),
			api.WrapHandler(s.templateHl.GetAll))

		templateApi.GET("/:id", s.authMW.PermissionCheck("shift.template.read"),
			api.WrapHandler(s.templateHl.GetByID))

		templateApi.POST("", s.authMW.PermissionCheck("shift.template.write"),
			api.WrapHandler(s.templateHl.Create))

		templateApi.PATCH("/:id", s.authMW.PermissionCheck("shift.template.write"),
			api.WrapHandler(s.templateHl.Update))

		templateApi.DELETE("/:id", s.authMW.PermissionCheck("shift.template.delete"),
			api.WrapHandler(s.templateHl.Delete))
	}

	// ==========================================
	// NHÓM QUY ĐỊNH CA LÀM (SHIFT RULES)
	// ==========================================
	ruleApi := r.Group("/shift-rules")
	ruleApi.Use(auth)
	{
		// Lấy cấu hình (ưu tiên theo storeId nếu có trong query)
		ruleApi.GET("", s.authMW.PermissionCheck("shift.rule.read"),
			api.WrapHandler(s.ruleHl.GetSettings))

		// Lưu cấu hình (Dùng PUT để Update-or-Insert)
		ruleApi.PUT("", s.authMW.PermissionCheck("shift.rule.write"),
			api.WrapHandler(s.ruleHl.Save))
	}

	// ==========================================
	// NHÓM LỊCH RẢNH/BẬN (AVAILABILITY)
	// ==========================================

	// --- GENERAL & MANAGER SIDE ---
	availabilityApi := r.Group("/shift-availabilities")
	availabilityApi.Use(auth)
	{
		// Manager lấy toàn bộ danh sách để kiểm soát
		availabilityApi.GET("", s.authMW.PermissionCheck("shift.availability.read"),
			api.WrapHandler(s.availabilityHl.GetAll))

		// Staff tạo mới đơn báo rảnh/bận
		availabilityApi.POST("", s.authMW.PermissionCheck("shift.availability.create"),
			api.WrapHandler(s.availabilityHl.Create))

		// Staff cập nhật hoặc xóa đơn của mình (ID-based)
		availabilityApi.PATCH("/:id", s.authMW.PermissionCheck("shift.availability.update"),
			api.WrapHandler(s.availabilityHl.Update))

		availabilityApi.DELETE("/:id", s.authMW.PermissionCheck("shift.availability.delete"),
			api.WrapHandler(s.availabilityHl.Delete))
	}

	// ==========================================
	// NHÓM APPLY CA
	// ==========================================
	applicationApi := r.Group("/shift-applications")
	applicationApi.Use(auth)
	{
		applicationApi.PATCH("/:id/review",
			s.authMW.PermissionCheck("shift.application.write"),
			api.WrapHandler(s.appHl.Review),
		)
	}

	// ==========================================
	// NHÓM ĐẶC BIỆT (STORES)
	// ==========================================
	storesApi := r.Group("/stores")
	storesApi.Use(auth)
	{
		storesApi.GET("/:id/shifts/:shiftId/candidates",
			s.authMW.PermissionCheck("shift.read"),
			api.WrapHandler(s.shiftHl.GetAvailableStaffs),
		)
	}
}
