package myshifts

import (
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
			s.authMW.PermissionCheck("me.current_shift"),
			s.shiftStaffHl.GetUserCurrentShift,
		)

		// 2. Check-in (Tác động lên Assignment)
		// URL: PUT /me/shifts/assignments/:id/check-in
		myShiftApi.PUT("/assignments/:id/check-in",
			s.authMW.PermissionCheck("me.check_in"),
			s.shiftStaffHl.CheckIn,
		)

		// 3. Check-out (Tác động lên Time Log)
		// URL: PUT /me/shifts/logs/:id/check-out
		myShiftApi.PUT("/logs/:id/check-out",
			s.authMW.PermissionCheck("me.check_out"),
			s.shiftStaffHl.CheckOut,
		)

		myShiftApi.GET("",
			s.authMW.PermissionCheck("me.agenda"),
			s.shiftStaffHl.GetAgenda)

		myShiftApi.GET("/time-logs",
			s.authMW.PermissionCheck("me.time_log_history"),
			s.shiftStaffHl.GetMyTimeLogHistory,
		)
	}

	availabilityApi := meApi.Group("/shift-availabilities")
	{
		// Lấy lịch sử đăng ký rảnh/bận của chính tôi
		availabilityApi.GET("", s.availabilityHl.GetMyHistory)
	}
}
