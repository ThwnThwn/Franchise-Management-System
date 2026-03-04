package membershiptiers

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (l *MembershipTierRouter) Init(r *gin.RouterGroup) {
	tierApi := r.Group("/membership-tiers")
	{
		// GET /membership-tiers
		tierApi.GET("", api.WrapHandler(l.tierHl.GetAllTiers))
	}

	// Áp dụng Auth Middleware cho toàn bộ group
	privateApi := tierApi.Group("")
	privateApi.Use(l.authMW.ValidateToken([]byte(l.cfg.JwtSecret)))
	{

		// POST /membership-tiers
		privateApi.POST("", l.authMW.PermissionCheck("loyalty-tier.create"),
			api.WrapHandler(l.tierHl.CreateTier))

		// PATCH /membership-tiers/:id
		privateApi.PATCH("/:id", l.authMW.PermissionCheck("loyalty-tier.update"),
			api.WrapHandler(l.tierHl.UpdateTier))

		// DELETE /membership-tiers/:id
		privateApi.DELETE("/:id", l.authMW.PermissionCheck("loyalty-tier.delete"),
			api.WrapHandler(l.tierHl.DeleteTier))
	}
}
