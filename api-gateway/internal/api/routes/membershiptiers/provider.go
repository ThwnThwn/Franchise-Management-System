package membershiptiers

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/engagement"
	"api-gateway/internal/api/middleware"
)

type MembershipTierRouter struct {
	cfg    *config.Config
	authMW *middleware.AuthMiddleware
	tierHl *engagement.MembershipTierHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	tierHl *engagement.MembershipTierHandler,
) *MembershipTierRouter {
	return &MembershipTierRouter{
		cfg:    cfg,
		authMW: authMW,
		tierHl: tierHl,
	}
}
