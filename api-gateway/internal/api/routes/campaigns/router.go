package campaigns

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (c *CampaignRouter) Init(r *gin.RouterGroup) {
	// Group chính: /campaigns
	campaignApi := r.Group("/campaigns")
	{
		// GET /campaigns
		campaignApi.GET("", api.WrapHandler(c.campaignHl.GetAll))

		// GET /campaigns/:id
		campaignApi.GET("/:id", api.WrapHandler(c.campaignHl.GetById))
	}

	privateApi := campaignApi.Group("")
	privateApi.Use(c.authMW.ValidateToken([]byte(c.cfg.JwtSecret)))
	{
		// POST /campaigns -> Tạo chiến dịch mới
		privateApi.POST("", c.authMW.PermissionCheck("campaign.create"),
			api.WrapHandler(c.campaignHl.Create))

		// PATCH /campaigns/:id/active -> Bật/tắt trạng thái chiến dịch
		privateApi.PATCH("/:id/active", c.authMW.PermissionCheck("campaign.update"),
			api.WrapHandler(c.campaignHl.UpdateActive))
	}
}
