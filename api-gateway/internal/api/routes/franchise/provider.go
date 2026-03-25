package franchise

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/franchise"
	"api-gateway/internal/api/middleware"
)

type FranchiseRouter struct {
	cfg         *config.Config
	authMW      *middleware.AuthMiddleware
	franchiseHl *franchise.FranchiseHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	franchiseHl *franchise.FranchiseHandler,
) *FranchiseRouter {
	return &FranchiseRouter{
		cfg:         cfg,
		authMW:      authMW,
		franchiseHl: franchiseHl,
	}
}
