package loyalty

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/engagement"
	"api-gateway/internal/api/middleware"
)

type LoyaltyRouter struct {
	cfg       *config.Config
	authMW    *middleware.AuthMiddleware
	loyaltyHl *engagement.LoyaltyHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	loyaltyHl *engagement.LoyaltyHandler,
) *LoyaltyRouter {
	return &LoyaltyRouter{
		cfg:       cfg,
		authMW:    authMW,
		loyaltyHl: loyaltyHl,
	}
}
