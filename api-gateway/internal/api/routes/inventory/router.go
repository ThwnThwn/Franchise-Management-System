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
	inventoryHandler *handlers.StoreIngredientHandler,
	authMW *middleware.AuthMiddleware,
) {
	ingredientApi := r.Group("/")
	{
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := ingredientApi.Group("")
	privateApi.Use(authMW.ValidateToken([]byte(accessTokenSecret)))
	{
		// thêm / giảm số lượng tồn kho
		privateApi.POST("/adjustments", authMW.PermissionCheck("inventory.adjustments"), inventoryHandler.IncreaseStock)
	}
}
