package permissions

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type PermissionRouter struct {
	cfg    *config.Config
	authMW *middleware.AuthMiddleware
	permHl *handlers.PermHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	permHl *handlers.PermHandler,
) *PermissionRouter {
	return &PermissionRouter{
		cfg:    cfg,
		authMW: authMW,
		permHl: permHl,
	}
}
