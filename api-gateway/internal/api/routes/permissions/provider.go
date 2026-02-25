package permissions

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/identity"
	"api-gateway/internal/api/middleware"
)

type PermissionRouter struct {
	cfg    *config.Config
	authMW *middleware.AuthMiddleware
	permHl *identity.PermHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	permHl *identity.PermHandler,
) *PermissionRouter {
	return &PermissionRouter{
		cfg:    cfg,
		authMW: authMW,
		permHl: permHl,
	}
}
