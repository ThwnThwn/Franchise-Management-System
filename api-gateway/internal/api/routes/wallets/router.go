package wallets

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (r *WalletRouter) Init(router *gin.RouterGroup) {
	// ----------------------------------------------------------------------
	// NHÓM API PUBLIC (Không cần token)
	// ----------------------------------------------------------------------
	publicWalletApi := router.Group("/wallets")
	{
		// Hứng callback từ VNPay
		publicWalletApi.GET("/vnpay/return", api.WrapHandler(r.walletHl.ProcessTopUpVnPayReturn))
	}

	// ----------------------------------------------------------------------
	// NHÓM API DÀNH CHO ADMIN / QUẢN LÝ (Cần quyền Admin/Manager)
	// ----------------------------------------------------------------------
	adminWalletApi := router.Group("/wallets")
	adminWalletApi.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		// Khai báo API cụ thể trước, API có tham số ID sau
		adminWalletApi.GET("/users/:userId", r.authMW.PermissionCheck("wallet.read"), api.WrapHandler(r.walletHl.GetWalletByUserId))
		adminWalletApi.PATCH("/:id/status", r.authMW.PermissionCheck("wallet.update"), api.WrapHandler(r.walletHl.UpdateWalletStatus))

		// Quản lý Giao dịch (Wallet Transactions)
		adminWalletApi.GET("/transactions", r.authMW.PermissionCheck("wallet.transaction.read"), api.WrapHandler(r.transHl.GetAllTransactions))
		// Tra cứu giao dịch (Dùng chung phân quyền read)
		adminWalletApi.GET("/transactions/:id", r.authMW.PermissionCheck("wallet.transaction.read"), api.WrapHandler(r.transHl.GetTransactionById))
		adminWalletApi.GET("/transactions/code/:code", r.authMW.PermissionCheck("wallet.transaction.read"), api.WrapHandler(r.transHl.GetTransactionByCode))
	}

	// ----------------------------------------------------------------------
	// NHÓM API DÀNH CHO END-USER (Tiền tố: /me/...)
	// Cần token nhưng chỉ check quyền cơ bản của người dùng
	// ----------------------------------------------------------------------
	myWalletApi := router.Group("/me/wallet")
	myWalletApi.Use(r.authMW.ValidateToken([]byte(r.cfg.JwtSecret)))
	{
		myWalletApi.GET("", r.authMW.PermissionCheck("wallet.read.me"),
			api.WrapHandler(r.walletHl.GetMyWallet))

		myWalletApi.POST("/top-up", r.authMW.PermissionCheck("wallet.topup.me"),
			api.WrapHandler(r.walletHl.TopUpMyWallet))

		myWalletApi.GET("/transactions", r.authMW.PermissionCheck("wallet.transaction.read.me"),
			api.WrapHandler(r.transHl.GetMyWalletTransactions))
	}
}
