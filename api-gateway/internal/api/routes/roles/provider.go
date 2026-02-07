package roles

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type RoleRouter struct {
	cfg    *config.Config
	authMW *middleware.AuthMiddleware
	roleHl *handlers.RoleHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	roleHl *handlers.RoleHandler,
) *RoleRouter {
	return &RoleRouter{
		cfg:    cfg,
		authMW: authMW,
		roleHl: roleHl,
	}
}
