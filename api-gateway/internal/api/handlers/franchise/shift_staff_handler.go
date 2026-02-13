package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/validation"
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftStaffHandler struct {
	shiftGW franchise.IShiftService
}

func NewShiftStaffHandler(shiftGW franchise.IShiftService) *ShiftStaffHandler {
	return &ShiftStaffHandler{shiftGW: shiftGW}
}

// GetUserCurrentShift
// @Summary      Get My Current Shift
// @Description  Retrieve the current active shift for the logged-in user.
// @Tags         Shifts Staff
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Success      200  {object}  map[string]interface{} "Current shift details"
// @Failure      401  {object}  map[string]string      "Unauthorized"
// @Router       /me/shifts/current [get]
func (h *ShiftStaffHandler) GetUserCurrentShift(c *gin.Context) {
	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrInvalidToken)
		return
	}

	ctx := c.Request.Context()
	res, err := h.shiftGW.GetUserCurrentShift(ctx, userID.(uuid.UUID))
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// CheckIn (Vào ca)
// @Summary      Staff Check-in
// @Description  Check-in to a specific shift assignment using GPS location.
// @Tags         Shifts Staff
// @Security     BearerAuth
// @Param        id   path      string  true  "Assignment ID"
// @Param        body body      shift.AttendancePayload true "GPS & Note"
// @Router       /me/shifts/assignments/{id}/check-in [put]
func (h *ShiftStaffHandler) CheckIn(c *gin.Context) {
	// 1. Lấy userID từ Token Middleware
	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrInvalidToken)
		return
	}

	// 2. Lấy assignmentId từ URL Path
	assignmentID, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	// 3. Bind JSON Payload (GPS & Note)
	var req shift.AttendancePayload
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(userID, assignmentID)

	// ctx := c.Request.Context()
	// err = h.shiftGW.CheckIn(ctx, userID.(uuid.UUID), assignmentID, req)
	// if err != nil {
	// 	c.Error(err)
	// 	return
	// }

	c.JSON(http.StatusOK, gin.H{})
}

// CheckOut (Tan ca)
// @Summary      Staff Check-out
// @Description  Check-out from an active shift log.
// @Tags         Shifts Staff
// @Security     BearerAuth
// @Param        id   path      string  true  "Log ID"
// @Param        body body      shift.AttendancePayload true "GPS & Note"
// @Router       /me/shifts/logs/{id}/check-out [put]
func (h *ShiftStaffHandler) CheckOut(c *gin.Context) {
	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrInvalidToken)
		return
	}

	// Lấy logId từ URL Path
	logID, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}
	var req shift.AttendancePayload
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(userID)
	fmt.Println(logID)
	fmt.Println(req)

	// ctx := c.Request.Context()
	// err = h.shiftGW.CheckOut(ctx, userID.(uuid.UUID), logID, req)
	// if err != nil {
	// 	c.Error(err)
	// 	return
	// }

	c.JSON(http.StatusOK, gin.H{})
}

// GetAgenda (Lịch cá nhân)
// @Summary      Get Staff Agenda
// @Description  Retrieve shift schedule within a date range for the logged-in staff.
// @Tags         Shifts Staff
// @Security     BearerAuth
// @Param        startDate query     string  true  "Start Date (YYYY-MM-DD)"
// @Param        endDate   query     string  true  "End Date (YYYY-MM-DD)"
// @Router       /me/shifts [get]
func (h *ShiftStaffHandler) GetAgenda(c *gin.Context) {
	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrInvalidToken)
		return
	}

	// Bind Query Params (startDate, endDate)
	var params shift.GetAgendaParams
	if err := c.ShouldBindQuery(&params); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(userID)

	// ctx := c.Request.Context()
	// res, err := h.shiftGW.GetAgenda(ctx, userID.(uuid.UUID), params)
	// if err != nil {
	// 	c.Error(err)
	// 	return
	// }

	res := gin.H{}

	c.JSON(http.StatusOK, res)
}
