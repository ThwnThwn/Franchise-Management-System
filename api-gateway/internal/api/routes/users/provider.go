package users

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/identity"
	"api-gateway/internal/api/middleware"
)

type UserRouter struct {
	cfg    *config.Config
	authMW *middleware.AuthMiddleware
	userHl *identity.UserHandler
}

func NewRouter(cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	userHl *identity.UserHandler,
) *UserRouter {
	return &UserRouter{
		cfg:    cfg,
		authMW: authMW,
		userHl: userHl,
	}
}
