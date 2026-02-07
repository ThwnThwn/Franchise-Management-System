package ingredients

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type StoreIngredientRouter struct{}

func (sI *StoreIngredientRouter) NewStoreIngredientRouter(cfg *config.Config,
	storeR *gin.RouterGroup,
	storeIngreHl *handlers.StoreIngredientHandler,
	authMW *middleware.AuthMiddleware,
) {
	storeIngreR := storeR.Group("")
	storeIngreR.Use(authMW.ValidateToken([]byte(cfg.JwtSecret)))

	storeIngreApi := storeIngreR.Group("/ingredients")
	{
		storeIngreApi.GET("", authMW.PermissionCheck("stock.read"), storeIngreHl.GetAllStock)
	}

	storeIngreIdApi := storeIngreR.Group("/:id/ingredients")
	{
		storeIngreIdApi.GET("", authMW.PermissionCheck("stock.read"), storeIngreHl.GetStockByStoreId)
		storeIngreIdApi.PUT("/adjustments",
			authMW.PermissionCheck("stock.update"), storeIngreHl.IncreaseStock)
	}
}
