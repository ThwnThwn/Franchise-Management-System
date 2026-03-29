package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/constants/enum"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftHandler struct {
	shiftGW franchise.IShiftService
	userGW  identity.IUserService
}

func NewShiftHandler(
	shiftGW franchise.IShiftService,
	userGW identity.IUserService,
) *ShiftHandler {
	return &ShiftHandler{
		shiftGW: shiftGW,
		userGW:  userGW,
	}
}

// =====================
// Shifts
// =====================

// GetAll (Lấy danh sách ca làm việc)
// @Summary      Get all shifts
// @Description  Retrieve a list of shifts with applied filters (Used for Calendar/List view).
// @Tags         Shifts
// @Security     BearerAuth
// @Produce      json
// @Param        query  query  shift.GetShiftsParams  false  "Filter parameters"
// @Success      200    {array}  shift.ShiftResponse "List of shifts"
// @Failure      400    {object} object "Validation Error (Bad Request)"
// @Failure      401    {object} object "Unauthorized"
// @Failure      500    {object} object "Internal Server Error"
// @Router       /shifts [get]
func (h *ShiftHandler) GetAll(c *gin.Context) error {
	var params shift.GetShiftsParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	res, err := h.shiftGW.GetShifts(c.Request.Context(), &params)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetByID (Lấy chi tiết ca làm)
// @Summary      Get shift details
// @Description  Retrieve detailed information about a specific shift and its assigned staff.
// @Tags         Shifts
// @Security     BearerAuth
// @Produce      json
// @Param        id   path   string  true  "Shift ID"
// @Success      200  {object} shift.ShiftResponse "Shift details"
// @Failure      400  {object} object "Invalid Shift ID (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      404  {object} object "Shift Not Found"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/{id} [get]
func (h *ShiftHandler) GetByID(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	res, err := h.shiftGW.GetById(c.Request.Context(), id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Create (Tạo ca làm mới)
// @Summary      Create a new shift
// @Description  Create a new working shift for the store.
// @Tags         Shifts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body   shift.CreateShiftRequest true "Shift information"
// @Success      200  {object} map[string]interface{} "Shift created successfully"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts [post]
func (h *ShiftHandler) Create(c *gin.Context) error {
	var req shift.CreateShiftRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	err = h.shiftGW.Create(ctx, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Update (Cập nhật ca làm - PATCH)
// @Summary      Update a shift
// @Description  Update shift timings or the required number of staff.
// @Tags         Shifts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path   string                   true  "Shift ID"
// @Param        body body   shift.UpdateShiftRequest true  "Update data"
// @Success      200  {object} map[string]interface{} "Shift updated successfully"
// @Failure      400  {object} object "Validation Error or Invalid ID"
// @Failure      401  {object} object "Unauthorized"
// @Failure      404  {object} object "Shift Not Found"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/{id} [patch]
func (h *ShiftHandler) Update(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.UpdateShiftRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	err = h.shiftGW.Update(ctx, id, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Delete (Xóa ca làm)
// @Summary      Delete a shift
// @Description  Delete a specific shift (Only allowed if it is in Draft or Planning status).
// @Tags         Shifts
// @Security     BearerAuth
// @Produce      json
// @Param        id   path   string  true  "Shift ID"
// @Success      200  {object} map[string]interface{} "Shift deleted successfully"
// @Failure      400  {object} object "Invalid ID or Bad Request"
// @Failure      401  {object} object "Unauthorized"
// @Failure      403  {object} object "Forbidden (Cannot delete published shift)"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/{id} [delete]
func (h *ShiftHandler) Delete(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	err = h.shiftGW.Delete(c.Request.Context(), id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Publish (Xuất bản ca lẻ)
// @Summary      Publish a single shift
// @Description  Change the shift status to Published, making it visible to employees on the schedule.
// @Tags         Shifts
// @Security     BearerAuth
// @Produce      json
// @Param        id   path   string  true  "Shift ID"
// @Success      200  {object} map[string]interface{} "Shift published successfully"
// @Failure      400  {object} object "Invalid ID or Bad Request"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/{id}/publish [put]
func (h *ShiftHandler) Publish(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	err = h.shiftGW.Publish(c.Request.Context(), id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// GetAvailableStaffs (Gợi ý nhân viên)
// @Summary      Get available staff candidates
// @Description  Check schedule conflicts and shift rules to suggest suitable staff members for a specific shift.
// @Tags         Shifts: Assignment
// @Security     BearerAuth
// @Produce      json
// @Param        id      path   string  true  "Store ID"
// @Param        shiftId path   string  true  "Shift ID"
// @Success      200     {array}  shift.StaffAssignmentCandidate "List of available staff"
// @Failure      400     {object} object "Invalid ID (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /stores/{id}/shifts/{shiftId}/candidates [get]
func (h *ShiftHandler) GetAvailableStaffs(c *gin.Context) error {
	storeID, err1 := uuid.Parse(c.Param("id"))
	shiftID, err2 := uuid.Parse(c.Param("shiftId"))
	if err1 != nil || err2 != nil {
		return errorcode.ErrInvalidId
	}

	res, err := h.shiftGW.GetAvailableStaffs(c.Request.Context(), storeID, shiftID)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateAssignments (Cập nhật phân công)
// @Summary      Update staff assignments
// @Description  Modify the list of employees assigned to a specific shift.
// @Tags         Shifts: Assignment
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id      path   string    true  "Shift ID"
// @Param        body    body   []string  true  "List of User IDs to assign"
// @Success      200     {object} shift.AssignStaffResponse "Assignment update result"
// @Failure      400     {object} object "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /shifts/{id}/assign [put]
func (h *ShiftHandler) UpdateAssignments(c *gin.Context) error {
	shiftID, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.AssignStaffRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctxUserId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	res, err := h.shiftGW.UpdateAssignments(c.Request.Context(), ctxUserId, shiftID, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// AutoFill
// @Summary      Auto-fill shift assignments
// @Description  System automatically calculates and assigns suitable staff to empty (Draft) shifts based on availability and scheduling rules.
// @Tags         Shifts: Assignment
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body shift.AutoFillShiftsRequest true "Time range for auto-filling shifts"
// @Success      200  {object} shift.AutoFillShiftsResponse "Auto-fill result summary"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/auto-fill [put]
func (h *ShiftHandler) AutoFill(c *gin.Context) error {
	var req shift.AutoFillShiftsRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	storeId, err := contextutils.GetStoreId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.shiftGW.AutoFill(ctx, storeId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// =====================
// Time Logs (Chấm công)
// =====================

// GetAllTimeLogs (Lấy danh sách nhật ký chấm công)
// @Summary      Get time logs
// @Description  Manager retrieves all time logs within the Store, applying optional filters.
// @Tags         Shifts: Time log
// @Security     BearerAuth
// @Produce      json
// @Param        query  query  shift.GetTimeLogsParams  false  "Time log filters"
// @Success      200    {object} common.PaginationResult[shift.TimeLogResponse] "Paginated list of time logs"
// @Failure      400    {object} object "Validation Error (Bad Request)"
// @Failure      401    {object} object "Unauthorized"
// @Failure      500    {object} object "Internal Server Error"
// @Router       /shifts/time-logs [get]
func (h *ShiftHandler) GetAllTimeLogs(c *gin.Context) error {
	var params shift.GetTimeLogsParams
	// 1. Bind Query Params từ URL
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	// 2. Gọi sang Franchise Service (qua shiftGW) lấy danh sách Logs
	// Giả sử trả về: *common.PageResult{ Items: []*shift.TimeLogResponse, Total: int }
	logRes, err := h.shiftGW.GetTimeLogs(ctx, &params)
	if err != nil {
		return err
	}

	// if len(logRes.Items) == 0 {
	//  c.JSON(http.StatusOK, logRes)
	//  return nil
	// }

	// 3. Gom danh sách ID nhân viên (Unique IDs) để chuẩn bị gọi Identity
	// staffIDMap := make(map[string]bool)
	// var staffIDs []string
	// for _, log := range logRes.Items {
	//  if log != nil && log.Staff.Id != "" && !staffIDMap[log.Staff.Id] {
	//      staffIDMap[log.Staff.Id] = true
	//      staffIDs = append(staffIDs, log.Staff.Id)
	//  }
	// }

	// 4. Dùng userGW gọi sang Identity Service lấy thông tin User chi tiết
	// users, err := h.userGW.GetByIds(ctx, staffIDs)
	// if err != nil {
	//  // Ở Gateway, nếu Identity "die" nhưng Franchise vẫn có data,
	//  // mình vẫn nên trả về logs kèm ID nhân viên thay vì báo lỗi 500 (Graceful Degradation).
	//  c.JSON(http.StatusOK, logRes)
	//  return nil
	// }

	// 5. Mapper Batch để trộn data từ users vào logRes.Items
	// Hàm mapper anh em mình vừa viết ở câu trước
	// logRes.Items = mapper.MapStaffToTimeLogsBatch(logRes.Items, users)

	c.JSON(http.StatusOK, logRes)
	return nil
}

// UpdateTimeLog (Duyệt/Cập nhật chấm công)
// @Summary      Review or update time log
// @Description  Manager approves (Valid) or rejects (Invalid) an employee's actual worked shift.
// @Tags         Shifts: Time log
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path   string true "Time log ID"
// @Param        body  body   shift.UpdateTimeLogRequest true "Update data"
// @Success      200   {object} map[string]interface{} "Time log updated successfully"
// @Failure      400   {object} object "Validation Error (e.g., missing rejection reason)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /shifts/time-logs/{id} [patch]
func (h *ShiftHandler) UpdateTimeLog(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.UpdateTimeLogRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	// Nếu status là Invalid (2) thì ManagerNote không được trống
	if req.Status == enum.TimeLogStatus_INVALID && (req.ManagerNote == nil || *req.ManagerNote == "") {
		return &errorcode.ErrorResponse{
			Status: http.StatusBadRequest,
			Title:  "Thao tác không thành công",
			Detail: "Phải nhập lý do khi từ chối chấm công",
		}
	}

	err = h.shiftGW.UpdateTimeLog(c.Request.Context(), id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}
