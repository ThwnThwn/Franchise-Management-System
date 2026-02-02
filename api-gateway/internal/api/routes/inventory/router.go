package inventory

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/domain/constants/roleslug"

	"github.com/gin-gonic/gin"
)

type InventoryRouter struct{}

func (a *InventoryRouter) NewInventoryRouter(cfg *config.Config,
	r *gin.RouterGroup,
	inventoryHandler *handlers.InventoryHandler,
	blackListUC abstractions.IBlacklistUseCase,
) {
	ingredientApi := r.Group("/inventory")
	{
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := ingredientApi.Group("")
	privateApi.Use(middleware.ValidateToken(blackListUC, []byte(accessTokenSecret)))
	{
	}

	managerApi := privateApi.Group("")
	managerApi.Use(middleware.RequireRoles(string(roleslug.Manager)))
	{
		// thêm / giảm số lượng tồn kho (hiện chỉ mới tăng)
		managerApi.POST("/adjustments", inventoryHandler.IncreaseStock)
	}
}
