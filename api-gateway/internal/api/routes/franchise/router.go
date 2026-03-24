package franchise

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (s *FranchiseRouter) Init(r *gin.RouterGroup) {
	// Middleware xác thực Token dùng chung
	auth := s.authMW.ValidateToken([]byte(s.cfg.JwtSecret))

	// ==========================================
	// 1. NHÓM WEBHOOKS
	// ==========================================
	webhookApi := r.Group("/franchise/webhook")
	{
		// DocuSeal bắn webhook về đây, bảo mật bằng header X-Docuseal-Signature
		webhookApi.POST("/docuseal", api.WrapHandler(s.franchiseHl.HandleDocuSealWebhook))
	}

	// ==========================================
	// 2. NHÓM HỒ SƠ ĐĂNG KÝ (APPLICATIONS)
	// ==========================================
	appApi := r.Group("/franchise/applications")
	appApi.Use(auth)
	{
		// Đối tác nộp hồ sơ (Ai có tài khoản Customer đều được nộp)
		appApi.POST("", s.authMW.PermissionCheck("franchise.application.submit"),
			api.WrapHandler(s.franchiseHl.SubmitApplication))

		// Admin xem danh sách & chi tiết
		appApi.GET("", s.authMW.PermissionCheck("franchise.application.read"),
			api.WrapHandler(s.franchiseHl.GetAllApplications))

		appApi.GET("/:id", s.authMW.PermissionCheck("franchise.application.read"),
			api.WrapHandler(s.franchiseHl.GetApplicationDetail))

		// Admin duyệt/từ chối hồ sơ
		appApi.PATCH("/:id/review", s.authMW.PermissionCheck("franchise.application.review"),
			api.WrapHandler(s.franchiseHl.ReviewApplication))
	}

	// ==========================================
	// 3. NHÓM HỢP ĐỒNG NHƯỢNG QUYỀN (CONTRACTS)
	// ==========================================
	contractApi := r.Group("/franchise/contracts")
	contractApi.Use(auth)
	{
		// Admin/Kế toán xem danh sách & chi tiết
		contractApi.GET("", s.authMW.PermissionCheck("franchise.contract.read"),
			api.WrapHandler(s.franchiseHl.GetAllContracts))

		contractApi.GET("/:id", s.authMW.PermissionCheck("franchise.contract.read"),
			api.WrapHandler(s.franchiseHl.GetContractDetail))

		// Kế toán xác nhận đóng tiền cọc
		contractApi.POST("/:id/payment", s.authMW.PermissionCheck("franchise.contract.payment"),
			api.WrapHandler(s.franchiseHl.ConfirmDepositPayment))

		// Admin cập nhật tiến độ thi công (Setup)
		contractApi.PATCH("/:id/setup", s.authMW.PermissionCheck("franchise.contract.setup"),
			api.WrapHandler(s.franchiseHl.UpdateSetupStatus))

		// Admin tạm ngưng / chấm dứt hợp đồng
		contractApi.PATCH("/:id/status", s.authMW.PermissionCheck("franchise.contract.write"),
			api.WrapHandler(s.franchiseHl.UpdateContractStatus))

		// Admin gia hạn hợp đồng
		contractApi.POST("/:id/renew", s.authMW.PermissionCheck("franchise.contract.write"),
			api.WrapHandler(s.franchiseHl.RenewContract))
	}
}
