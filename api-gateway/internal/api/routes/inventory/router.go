package inventory

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type InventoryRouter struct{}

func (a *InventoryRouter) NewInventoryRouter(cfg *config.Config,
	r *gin.RouterGroup,
	inventoryHandler *handlers.InventoryHandler,
	authMW *middleware.AuthMiddleware,
) {
	ingredientApi := r.Group("/inventory")
	{
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := ingredientApi.Group("")
	privateApi.Use(authMW.ValidateToken([]byte(accessTokenSecret)))
	{
	}

	// managerApi := privateApi.Group("")
	// managerApi.Use(middleware.RequireRoles(string(roleslug.Manager)))
	{
		// thêm / giảm số lượng tồn kho (hiện chỉ mới tăng)
		privateApi.POST("/adjustments", authMW.PermissionCheck("inventory.adjustments"), inventoryHandler.IncreaseStock)
	}
}
