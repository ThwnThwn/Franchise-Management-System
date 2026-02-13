package myshifts

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/franchise"
	"api-gateway/internal/api/middleware"
)

type MyShiftsRouter struct {
	cfg          *config.Config
	authMW       *middleware.AuthMiddleware
	shiftStaffHl *franchise.ShiftStaffHandler
}

func NewRouter(cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	shiftStaffHl *franchise.ShiftStaffHandler,
) *MyShiftsRouter {
	return &MyShiftsRouter{
		cfg:          cfg,
		authMW:       authMW,
		shiftStaffHl: shiftStaffHl,
	}
}
