package campaigns

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/engagement"
	"api-gateway/internal/api/middleware"
)

type CampaignRouter struct {
	cfg        *config.Config
	authMW     *middleware.AuthMiddleware
	campaignHl *engagement.CampaignHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	campaignHl *engagement.CampaignHandler,
) *CampaignRouter {
	return &CampaignRouter{
		cfg:        cfg,
		authMW:     authMW,
		campaignHl: campaignHl,
	}
}
