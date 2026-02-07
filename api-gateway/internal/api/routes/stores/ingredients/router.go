package ingredients

import (
	"github.com/gin-gonic/gin"
)

func (i *StoreIngredientRouter) Init(storeR *gin.RouterGroup) {
	storeIngreR := storeR.Group("")
	storeIngreR.Use(i.authMW.ValidateToken([]byte(i.cfg.JwtSecret)))

	storeIngreApi := storeIngreR.Group("/ingredients")
	{
		storeIngreApi.GET("", i.authMW.PermissionCheck("stock.read"), i.storeIngreHl.GetAllStock)
	}

	storeIngreIdApi := storeIngreR.Group("/:id/ingredients")
	{
		storeIngreIdApi.GET("", i.authMW.PermissionCheck("stock.read"), i.storeIngreHl.GetStockByStoreId)
		storeIngreIdApi.PUT("/adjustments",
			i.authMW.PermissionCheck("stock.update"), i.storeIngreHl.IncreaseStock)
	}
}
