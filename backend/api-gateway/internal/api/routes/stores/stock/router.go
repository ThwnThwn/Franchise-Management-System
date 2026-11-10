package stock

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (i *StockRouter) Init(storeR *gin.RouterGroup) {
	stockApi := storeR.Group("")
	stockApi.Use(i.authMW.ValidateToken([]byte(i.cfg.JwtSecret)))

	storeIngreApi := stockApi.Group("/stock")
	{
		storeIngreApi.GET("", i.authMW.PermissionCheck("stock.read"),
			api.WrapHandler(i.stockHl.GetAllStock))
	}

	storeIngreIdApi := stockApi.Group("/:id/stock")
	{
		storeIngreIdApi.GET("", i.authMW.PermissionCheck("stock.read"),
			api.WrapHandler(i.stockHl.GetStockByStoreId))

		storeIngreIdApi.PUT("", i.authMW.PermissionCheck("stock.update"),
			api.WrapHandler(i.stockHl.IncreaseStock))
	}
}
