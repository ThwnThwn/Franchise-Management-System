package myshifts

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
)

type MyShiftsRouter struct {
	cfg     *config.Config
	authMW  *middleware.AuthMiddleware
	shiftHl *handlers.ShiftHandler
}

func NewRouter(cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	shiftHl *handlers.ShiftHandler,
) *MyShiftsRouter {
	return &MyShiftsRouter{
		cfg:     cfg,
		authMW:  authMW,
		shiftHl: shiftHl,
	}
}
