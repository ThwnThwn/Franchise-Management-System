package myshifts

import (
	"github.com/gin-gonic/gin"
)

func (s *MyShiftsRouter) Init(r *gin.RouterGroup) {
	myShiftApi := r.Group("/me/shifts")
	myShiftApi.Use(s.authMW.ValidateToken([]byte(s.cfg.JwtSecret)))
	{
		// 1. Lấy ca hiện tại
		myShiftApi.GET("/current",
			s.authMW.PermissionCheck("me.current_shift"),
			s.shiftHl.GetUserCurrentShift,
		)

		// 2. Check-in (Tác động lên Assignment)
		// URL: PUT /me/shifts/assignments/:id/check-in
		myShiftApi.PUT("/assignments/:id/check-in",
			s.authMW.PermissionCheck("me.check_in"),
			// s.shiftHl.CheckIn,
		)

		// 3. Check-out (Tác động lên Time Log)
		// URL: PUT /me/shifts/logs/:id/check-out
		myShiftApi.PUT("/logs/:id/check-out",
			s.authMW.PermissionCheck("me.check_out"),
			// s.shiftHl.CheckOut,
		)
	}
}
