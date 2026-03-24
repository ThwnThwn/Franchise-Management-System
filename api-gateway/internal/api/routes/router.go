package routes

import (
	"api-gateway/config"
	"api-gateway/internal/api/middleware"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
	"golang.org/x/time/rate"
)

func NewEngine(cfg *config.Config, aR *AppRouter) *gin.Engine {
	r := gin.Default()
	// r.Use(gin.Recovery())

	// Apply Global Middleware
	r.Use(middleware.GlobalErrorHandler())

	r.Use(middleware.CORSMiddleware())
	r.Use(middleware.GlobalTimeoutMiddleware(time.Duration(cfg.GatewayTimeoutSeconds) * time.Second))

	r.Use(middleware.RateLimitMiddleware(rate.Limit(cfg.RateLimitPerSec), cfg.RateLimitBurst))
	middleware.StartCleanupJob(5*time.Minute, 1*time.Minute)

	// Health check cho Gateway
	r.GET("/health", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{
			"status": "UP",
			"port":   cfg.Port,
		})
	})

	r.Static("/api-docs", "./docs")
	r.StaticFile("/swagger", "./docs/index.html")

	// Truy cập: http://localhost:8080/swagger/index.html
	r.GET("/swagger/*any", func(c *gin.Context) {
		c.File("./docs/index.html")
	})

	// r.GET("/swagger/*any", ginSwagger.WrapHandler(
	// 	swaggerFiles.Handler,
	// 	ginSwagger.DocExpansion("none"),
	// 	ginSwagger.URL("/api-docs/engagement/engagement_swagger.json"),
	// 	ginSwagger.URL("/api-docs/franchise/franchise_swagger.json"),
	// 	ginSwagger.URL("/api-docs/identity/identity_swagger.json"),
	// 	ginSwagger.URL("/api-docs/inventory/inventory_swagger.json"),
	// 	ginSwagger.URL("/api-docs/product/product_swagger.json"),
	// 	ginSwagger.URL("/api-docs/sales/sales_swagger.json"),
	// 	ginSwagger.URL("/api-docs/supplier/supplier_swagger.json"),
	// 	ginSwagger.URL("/api-docs/vnpay/vnpay_swagger.json"),
	// ))

	MainGroup := r.Group("/api")
	{
		aR.VnPayRouter.Init(MainGroup)

		// Engagement
		aR.LoyaltyRouter.Init(MainGroup)
		aR.MembershipTierRouter.Init(MainGroup)
		aR.PointLogRouter.Init(MainGroup)
		aR.VoucherRouter.Init(MainGroup)
		aR.CampaignRouter.Init(MainGroup)

		// Franchise
		aR.StoreRouter.Init(MainGroup)
		aR.MyShiftsRouter.Init(MainGroup)
		aR.ShiftsRouter.Init(MainGroup)
		aR.FranchiseRouter.Init(MainGroup)

		// Identity
		aR.AuthRouter.Init(MainGroup)
		aR.UserRouter.Init(MainGroup)
		aR.RoleRouter.Init(MainGroup)
		aR.PermissionRouter.Init(MainGroup)

		// Inventory
		aR.IngredientRouter.Init(MainGroup)
		aR.RecipeRouter.Init(MainGroup)

		// Product
		aR.ProductRouter.Init(MainGroup)
		aR.CategoryRouter.Init(MainGroup)
		aR.VariantRouter.Init(MainGroup)

		// Sales
		aR.OrderRouter.Init(MainGroup)
		aR.CartRouter.Init(MainGroup)
		aR.WalletRouter.Init(MainGroup)
		aR.RefundRouter.Init(MainGroup)
		aR.WithdrawalRouter.Init(MainGroup)

		// Supplier
		aR.SupplierRouter.Init(MainGroup)
	}

	return r
}
