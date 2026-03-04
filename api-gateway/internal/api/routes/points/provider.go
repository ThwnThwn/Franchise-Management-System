package points

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/engagement"
	"api-gateway/internal/api/middleware"
)

type PointLogRouter struct {
	cfg     *config.Config
	authMW  *middleware.AuthMiddleware
	pointHl *engagement.PointLogHandler
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	pointHl *engagement.PointLogHandler,
) *PointLogRouter {
	return &PointLogRouter{
		cfg:     cfg,
		authMW:  authMW,
		pointHl: pointHl,
	}
}
