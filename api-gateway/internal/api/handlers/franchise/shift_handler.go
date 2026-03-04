package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
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
// @Summary      Lấy danh sách ca làm việc
// @Description  Truy xuất danh sách ca làm việc kèm bộ lọc (Dùng cho Calendar/List view).
// @Tags         Shifts
// @Security     BearerAuth
// @Param        storeId   query     string  true   "ID cửa hàng"
// @Param        fromDate  query     string  false  "Từ ngày (YYYY-MM-DD)"
// @Param        toDate    query     string  false  "Đến ngày (YYYY-MM-DD)"
// @Param        status    query     int     false  "Trạng thái ca"
// @Success      200       {array}   shift.ShiftResponse
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
// @Summary      Lấy chi tiết một ca làm
// @Description  Lấy thông tin chi tiết ca làm và danh sách nhân viên đã gán.
// @Tags         Shifts
// @Security     BearerAuth
// @Param        id   path      string  true  "ID ca làm"
// @Success      200  {object}  shift.ShiftResponse
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
// @Summary      Tạo ca làm mới
// @Description  Tạo một ca làm việc mới cho cửa hàng.
// @Tags         Shifts
// @Security     BearerAuth
// @Param        body body      shift.CreateShiftRequest true "Thông tin ca làm"
// @Router       /shifts [post]
func (h *ShiftHandler) Create(c *gin.Context) error {
	var req shift.CreateShiftRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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
// @Summary      Cập nhật ca làm việc
// @Description  Cập nhật giờ giấc hoặc số lượng nhân viên cần thiết cho ca làm.
// @Tags         Shifts
// @Security     BearerAuth
// @Param        id    path      string                   true  "ID ca làm"
// @Param        body  body      shift.UpdateShiftRequest true  "Dữ liệu cập nhật"
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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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
// @Summary      Xóa ca làm việc
// @Description  Xóa ca làm (Chỉ khi ở trạng thái Draft/Planning).
// @Tags         Shifts
// @Security     BearerAuth
// @Param        id   path      string  true  "ID ca làm"
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
// @Summary      Xuất bản ca làm
// @Description  Chuyển trạng thái sang Published để nhân viên thấy được lịch.
// @Tags         Shifts
// @Param        id   path      string  true  "ID ca làm"
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
// @Summary      Lấy danh sách ứng viên khả dụng
// @Description  Kiểm tra xung đột lịch và quy định để gợi ý nhân viên cho ca làm.
// @Tags         Shifts: Assignment
// @Param        id      path      string  true  "ID cửa hàng"
// @Param        shiftId path      string  true  "ID ca làm"
// @Success      200     {array}   shift.StaffAssignmentCandidate
// @Router       /stores/{storeId}/shifts/{shiftId}/candidates [get]
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
// @Summary      Cập nhật phân công nhân sự
// @Description  Thay đổi danh sách nhân viên được gán vào ca làm việc.
// @Tags         Shifts: Assignment
// @Param        shiftId path      string    true  "ID ca làm"
// @Param        body    body      []string  true  "Danh sách User ID"
// @Router       /shifts/{shiftId}/assign [put]
func (h *ShiftHandler) UpdateAssignments(c *gin.Context) error {
	shiftID, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var userIDs []uuid.UUID
	if err := c.ShouldBindJSON(&userIDs); err != nil {
		return validation.TranslateValidationError(err)
	}

	err = h.shiftGW.UpdateAssignments(c.Request.Context(), shiftID, userIDs)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// =====================
// Time Logs (Chấm công)
// =====================

// GetAllTimeLogs (Lấy danh sách nhật ký chấm công)
// @Summary      Lấy danh sách chấm công
// @Description  Manager lấy toàn bộ nhật ký chấm công trong Store kèm bộ lọc.
// @Tags         Shifts: Time log
// @Security     BearerAuth
// @Param        query  query    shift.GetTimeLogsParams  false  "Bộ lọc chấm công"
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
	// 	c.JSON(http.StatusOK, logRes)
	// 	return nil
	// }

	// 3. Gom danh sách ID nhân viên (Unique IDs) để chuẩn bị gọi Identity
	// staffIDMap := make(map[string]bool)
	// var staffIDs []string
	// for _, log := range logRes.Items {
	// 	if log != nil && log.Staff.Id != "" && !staffIDMap[log.Staff.Id] {
	// 		staffIDMap[log.Staff.Id] = true
	// 		staffIDs = append(staffIDs, log.Staff.Id)
	// 	}
	// }

	// 4. Dùng userGW gọi sang Identity Service lấy thông tin User chi tiết
	// users, err := h.userGW.GetByIds(ctx, staffIDs)
	// if err != nil {
	// 	// Ở Gateway, nếu Identity "die" nhưng Franchise vẫn có data,
	// 	// mình vẫn nên trả về logs kèm ID nhân viên thay vì báo lỗi 500 (Graceful Degradation).
	// 	c.JSON(http.StatusOK, logRes)
	// 	return nil
	// }

	// 5. Mapper Batch để trộn data từ users vào logRes.Items
	// Hàm mapper anh em mình vừa viết ở câu trước
	// logRes.Items = mapper.MapStaffToTimeLogsBatch(logRes.Items, users)

	c.JSON(http.StatusOK, logRes)
	return nil
}

// UpdateTimeLog (Duyệt/Cập nhật chấm công)
// @Summary      Duyệt hoặc cập nhật chấm công
// @Description  Manager phê duyệt (Valid) hoặc từ chối (Invalid) ca làm thực tế của nhân viên.
// @Tags         Shifts: Time log
// @Security     BearerAuth
// @Param        id    path      string  true  "ID nhật ký chấm công"
// @Param        body  body      shift.UpdateTimeLogRequest true "Dữ liệu cập nhật"
// @Success      200   {object}  gin.H
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
