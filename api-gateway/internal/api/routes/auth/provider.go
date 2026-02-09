package auth

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type AuthRouter struct {
	cfg    *config.Config
	authMW *middleware.AuthMiddleware
	authHl *handlers.AuthHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	authHl *handlers.AuthHandler,
) *AuthRouter {
	return &AuthRouter{
		cfg:    cfg,
		authMW: authMW,
		authHl: authHl,
	}
}
