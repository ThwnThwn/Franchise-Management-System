package myshifts

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (s *MyShiftsRouter) Init(r *gin.RouterGroup) {
	auth := s.authMW.ValidateToken([]byte(s.cfg.JwtSecret))

	meApi := r.Group("/me")
	meApi.Use(auth)

	myShiftApi := meApi.Group("/shifts")
	{
		// 1. Lấy ca hiện tại
		myShiftApi.GET("/current",
			s.authMW.PermissionCheck("me.current-shift.read"),
			api.WrapHandler(s.shiftStaffHl.GetUserCurrentShift),
		)

		// 2. Check-in (Tác động lên Assignment)
		// URL: PUT /me/shifts/assignments/:id/check-in
		myShiftApi.PUT("/assignments/:id/check-in",
			s.authMW.PermissionCheck("me.check-in"),
			api.WrapHandler(s.shiftStaffHl.CheckIn),
		)

		// 3. Check-out (Tác động lên Time Log)
		// URL: PUT /me/shifts/logs/:id/check-out
		myShiftApi.PUT("/logs/:id/check-out",
			s.authMW.PermissionCheck("me.check-out"),
			api.WrapHandler(s.shiftStaffHl.CheckOut),
		)

		myShiftApi.GET("",
			s.authMW.PermissionCheck("me.agenda.read"),
			api.WrapHandler(s.shiftStaffHl.GetMyAgenda))

		myShiftApi.GET("/time-logs",
			s.authMW.PermissionCheck("me.time-log-history.read"),
			api.WrapHandler(s.shiftStaffHl.GetMyTimeLogHistory),
		)
	}

	availabilityApi := meApi.Group("/shift-availabilities")
	{
		availabilityApi.GET("", s.authMW.PermissionCheck("me.shift.availability.read"),
			api.WrapHandler(s.availabilityHl.GetMyHistory))
	}

	swapApi := meApi.Group("/shift-swaps")
	{
		swapApi.GET("", s.authMW.PermissionCheck("me.shift.swap.read"),
			api.WrapHandler(s.swapHl.GetMySwaps))
	}
}
