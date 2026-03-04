package loyalty

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (l *LoyaltyRouter) Init(r *gin.RouterGroup) {
	loyaltyApi := r.Group("/loyalty")

	privateApi := loyaltyApi.Group("")
	privateApi.Use(l.authMW.ValidateToken([]byte(l.cfg.JwtSecret)))
	{
		privateApi.GET("/accounts", l.authMW.PermissionCheck("loyalty-account.read"),
			api.WrapHandler(l.loyaltyHl.GetLoyaltyAccounts))

		privateApi.GET("/reports", l.authMW.PermissionCheck("loyalty-account.read"),
			api.WrapHandler(l.loyaltyHl.GetLoyaltyReport))
	}
}
