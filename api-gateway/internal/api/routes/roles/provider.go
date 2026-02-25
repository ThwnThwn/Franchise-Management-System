package roles

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/identity"
	"api-gateway/internal/api/middleware"
)

type RoleRouter struct {
	cfg    *config.Config
	authMW *middleware.AuthMiddleware
	roleHl *identity.RoleHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	roleHl *identity.RoleHandler,
) *RoleRouter {
	return &RoleRouter{
		cfg:    cfg,
		authMW: authMW,
		roleHl: roleHl,
	}
}
