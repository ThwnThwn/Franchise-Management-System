package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/application/gateways/identity"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftStaffHandler struct {
	shiftGW franchise.IShiftService
	userGW  identity.IUserService
}

func NewShiftStaffHandler(
	shiftGW franchise.IShiftService,
	userGW identity.IUserService,
) *ShiftStaffHandler {
	return &ShiftStaffHandler{
		shiftGW: shiftGW,
		userGW:  userGW,
	}
}

// GetUserCurrentShift
// @Summary      Get current active shift
// @Description  Retrieve the current active shift details for the logged-in user.
// @Tags         Shifts: Staff
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Success      200  {object} shift.UserCurrentShiftResponse "Current shift details"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /me/shifts/current [get]
func (h *ShiftStaffHandler) GetUserCurrentShift(c *gin.Context) error {
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.shiftGW.GetUserCurrentShift(ctx, userId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// CheckIn
// @Summary      Staff check-in
// @Description  Check-in to a specific shift assignment using GPS location data.
// @Tags         Shifts: Staff
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path     string true "Assignment ID"
// @Param        body body     shift.AttendancePayload true "GPS coordinates and notes"
// @Success      200  {object} map[string]interface{} "Check-in successful"
// @Failure      400  {object} object "Validation Error or Invalid ID"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /me/shifts/assignments/{id}/check-in [put]
func (h *ShiftStaffHandler) CheckIn(c *gin.Context) error {
	// 1. Lấy userID từ Token Middleware
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	// 2. Lấy assignmentId từ URL Path
	assignmentID, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	// 3. Bind JSON Payload (GPS & Note)
	var req shift.AttendancePayload
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.shiftGW.CheckIn(ctx, userId, assignmentID, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// CheckOut
// @Summary      Staff check-out
// @Description  Check-out from an active shift log using GPS location data.
// @Tags         Shifts: Staff
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path     string true "Time Log ID"
// @Param        body body     shift.AttendancePayload true "GPS coordinates and notes"
// @Success      200  {object} map[string]interface{} "Check-out successful"
// @Failure      400  {object} object "Validation Error or Invalid ID"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /me/shifts/logs/{id}/check-out [put]
func (h *ShiftStaffHandler) CheckOut(c *gin.Context) error {
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	// Lấy logId từ URL Path
	logID, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}
	var req shift.AttendancePayload
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.shiftGW.CheckOut(ctx, userId, logID, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// GetMyAgenda
// @Summary      Get staff shift agenda
// @Description  Retrieve the shift schedule within a specified date range for the logged-in staff member.
// @Tags         Shifts: Staff
// @Security     BearerAuth
// @Produce      json
// @Param        startDate query  string true "Start Date (YYYY-MM-DD)"
// @Param        endDate   query  string true "End Date (YYYY-MM-DD)"
// @Success      200       {array}  shift.StaffShiftAgenda "List of scheduled shifts"
// @Failure      400       {object} object "Validation Error (Bad Request)"
// @Failure      401       {object} object "Unauthorized"
// @Failure      500       {object} object "Internal Server Error"
// @Router       /me/shifts [get]
func (h *ShiftStaffHandler) GetMyAgenda(c *gin.Context) error {
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	// Bind Query Params (startDate, endDate)
	var params shift.GetAgendaParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.shiftGW.GetMyAgenda(ctx, userId, &params)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetMyTimeLogHistory
// @Summary      Get my time log history
// @Description  Staff views their own time log history. The Gateway automatically binds the StaffID from the Token to ensure security.
// @Tags         Shifts: Staff
// @Security     BearerAuth
// @Produce      json
// @Param        query query  shift.GetTimeLogsParams false "Time log filters and status"
// @Success      200   {object} common.PaginationResult[shift.TimeLogResponse] "Paginated time log history"
// @Failure      400   {object} object "Validation Error (Bad Request)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /me/shifts/time-logs [get]
func (h *ShiftStaffHandler) GetMyTimeLogHistory(c *gin.Context) error {
	ctx := c.Request.Context()

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	var params shift.GetTimeLogsParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	// 3. SECURITY: Ép staffId phải là của chính mình
	params.StaffId = stringutils.ToPtr(userId.String())

	// 4. Gọi sang Franchise Service lấy logs
	logRes, err := h.shiftGW.GetTimeLogs(ctx, &params)
	if err != nil {
		return err
	}

	// me, err := h.userGW.GetById(ctx, userUUID)
	// if err == nil {
	//  users := []*user.UserResponse{me}
	//  logRes.Items = mapper.MapStaffToTimeLogsBatch(logRes.Items, users)
	// }

	c.JSON(http.StatusOK, logRes)
	return nil
}
