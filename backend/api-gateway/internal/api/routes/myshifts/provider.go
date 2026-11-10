package myshifts

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/franchise"
	"api-gateway/internal/api/middleware"
)

type MyShiftsRouter struct {
	cfg            *config.Config
	authMW         *middleware.AuthMiddleware
	shiftStaffHl   *franchise.ShiftStaffHandler
	availabilityHl *franchise.ShiftAvailabilityHandler
	swapHl         *franchise.ShiftSwapHandler
}

func NewRouter(cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	shiftStaffHl *franchise.ShiftStaffHandler,
	availabilityHl *franchise.ShiftAvailabilityHandler,
	swapHl *franchise.ShiftSwapHandler,
) *MyShiftsRouter {
	return &MyShiftsRouter{
		cfg:            cfg,
		authMW:         authMW,
		shiftStaffHl:   shiftStaffHl,
		availabilityHl: availabilityHl,
		swapHl:         swapHl,
	}
}
