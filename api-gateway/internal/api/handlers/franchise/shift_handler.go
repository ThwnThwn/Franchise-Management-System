package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/errormodel"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/mapper"
	"api-gateway/pkg/utils/validation"
	"fmt"
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
func (h *ShiftHandler) GetAll(c *gin.Context) {
	var params shift.GetShiftsParams
	if err := c.ShouldBindQuery(&params); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(params)
	// res, err := h.shiftGW.GetAll(c.Request.Context(), params)

	res := []shift.ShiftResponse{}
	c.JSON(http.StatusOK, res)
}

// GetByID (Lấy chi tiết ca làm)
// @Summary      Lấy chi tiết một ca làm
// @Description  Lấy thông tin chi tiết ca làm và danh sách nhân viên đã gán.
// @Tags         Shifts
// @Security     BearerAuth
// @Param        id   path      string  true  "ID ca làm"
// @Success      200  {object}  shift.ShiftResponse
// @Router       /shifts/{id} [get]
func (h *ShiftHandler) GetByID(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	fmt.Println(id)
	// res, err := h.shiftGW.GetByID(c.Request.Context(), id)

	res := shift.ShiftResponse{}
	c.JSON(http.StatusOK, res)
}

// Create (Tạo ca làm mới)
// @Summary      Tạo ca làm mới
// @Description  Tạo một ca làm việc mới cho cửa hàng.
// @Tags         Shifts
// @Security     BearerAuth
// @Param        body body      shift.CreateShiftRequest true "Thông tin ca làm"
// @Router       /shifts [post]
func (h *ShiftHandler) Create(c *gin.Context) {
	var req shift.CreateShiftRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(req)
	// err := h.shiftGW.Create(c.Request.Context(), req)

	c.JSON(http.StatusOK, gin.H{})
}

// Update (Cập nhật ca làm - PATCH)
// @Summary      Cập nhật ca làm việc
// @Description  Cập nhật giờ giấc hoặc số lượng nhân viên cần thiết cho ca làm.
// @Tags         Shifts
// @Security     BearerAuth
// @Param        id    path      string                   true  "ID ca làm"
// @Param        body  body      shift.UpdateShiftRequest true  "Dữ liệu cập nhật"
// @Router       /shifts/{id} [patch]
func (h *ShiftHandler) Update(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req shift.UpdateShiftRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(id, req)
	// err := h.shiftGW.Update(c.Request.Context(), id, req)

	c.JSON(http.StatusOK, gin.H{})
}

// Delete (Xóa ca làm)
// @Summary      Xóa ca làm việc
// @Description  Xóa ca làm (Chỉ khi ở trạng thái Draft/Planning).
// @Tags         Shifts
// @Security     BearerAuth
// @Param        id   path      string  true  "ID ca làm"
// @Router       /shifts/{id} [delete]
func (h *ShiftHandler) Delete(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	fmt.Println(id)
	// err := h.shiftGW.Delete(c.Request.Context(), id)

	c.JSON(http.StatusOK, gin.H{})
}

// Publish (Xuất bản ca lẻ)
// @Summary      Xuất bản ca làm
// @Description  Chuyển trạng thái sang Published để nhân viên thấy được lịch.
// @Tags         Shifts
// @Param        id   path      string  true  "ID ca làm"
// @Router       /shifts/{id}/publish [put]
func (h *ShiftHandler) Publish(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	fmt.Println(id)
	// err := h.shiftGW.Publish(c.Request.Context(), id)

	c.JSON(http.StatusOK, gin.H{})
}

// BulkPublish (Xuất bản hàng loạt)
// @Summary      Xuất bản nhiều ca làm
// @Description  Công khai danh sách ca làm việc cùng lúc.
// @Tags         Shifts
// @Param        body body      object{ids=[]string} true "Danh sách ID ca làm"
// @Router       /shifts/bulk-publish [post]
func (h *ShiftHandler) BulkPublish(c *gin.Context) {
	var req struct {
		IDs []uuid.UUID `json:"ids" binding:"required"`
	}

	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(req.IDs)
	// err := h.shiftGW.BulkPublish(c.Request.Context(), req.IDs)

	c.JSON(http.StatusOK, gin.H{})
}

// GetAvailableStaffs (Gợi ý nhân viên)
// @Summary      Lấy danh sách ứng viên khả dụng
// @Description  Kiểm tra xung đột lịch và quy định để gợi ý nhân viên cho ca làm.
// @Tags         Shifts: Assignment
// @Param        storeId path      string  true  "ID cửa hàng"
// @Param        shiftId path      string  true  "ID ca làm"
// @Success      200     {array}   shift.StaffAssignmentCandidate
// @Router       /stores/{storeId}/shifts/{shiftId}/candidates [get]
func (h *ShiftHandler) GetAvailableStaffs(c *gin.Context) {
	storeID, err1 := uuid.Parse(c.Param("storeId"))
	shiftID, err2 := uuid.Parse(c.Param("shiftId"))
	if err1 != nil || err2 != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	fmt.Println(storeID, shiftID)
	// res, err := h.shiftGW.GetAvailableStaffs(c.Request.Context(), storeID, shiftID)

	res := []shift.StaffAssignmentCandidate{}
	c.JSON(http.StatusOK, res)
}

// UpdateAssignments (Cập nhật phân công)
// @Summary      Cập nhật phân công nhân sự
// @Description  Thay đổi danh sách nhân viên được gán vào ca làm việc.
// @Tags         Shifts: Assignment
// @Param        shiftId path      string    true  "ID ca làm"
// @Param        body    body      []string  true  "Danh sách User ID"
// @Router       /shifts/{shiftId}/assign [put]
func (h *ShiftHandler) UpdateAssignments(c *gin.Context) {
	shiftID, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var userIDs []uuid.UUID
	if err := c.ShouldBindJSON(&userIDs); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(shiftID, userIDs)
	// err := h.shiftGW.UpdateAssignments(c.Request.Context(), shiftID, userIDs)

	c.JSON(http.StatusOK, gin.H{})
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
func (h *ShiftHandler) GetAllTimeLogs(c *gin.Context) {
	var params shift.GetTimeLogsParams
	// 1. Bind Query Params từ URL
	if err := c.ShouldBindQuery(&params); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()

	// 2. Gọi sang Franchise Service (qua shiftGW) lấy danh sách Logs
	// Giả sử trả về: *common.PageResult{ Items: []*shift.TimeLogResponse, Total: int }
	// logRes, err := h.shiftGW.GetAllTimeLogs(ctx, params)
	// if err != nil {
	// 	c.Error(err)
	// 	return
	// }
	logRes := &common.PaginationResult[*shift.TimeLogResponse]{}
	if len(logRes.Items) == 0 {
		c.JSON(http.StatusOK, logRes)
		return
	}

	// 3. Gom danh sách ID nhân viên (Unique IDs) để chuẩn bị gọi Identity
	staffIDMap := make(map[string]bool)
	var staffIDs []string
	for _, log := range logRes.Items {
		if log != nil && log.Staff.ID != "" && !staffIDMap[log.Staff.ID] {
			staffIDMap[log.Staff.ID] = true
			staffIDs = append(staffIDs, log.Staff.ID)
		}
	}

	// 4. Dùng userGW gọi sang Identity Service lấy thông tin User chi tiết
	users, err := h.userGW.GetByIds(ctx, staffIDs)
	if err != nil {
		// Ở Gateway, nếu Identity "tèo" nhưng Franchise vẫn có data,
		// mình vẫn nên trả về logs kèm ID nhân viên thay vì báo lỗi 500 (Graceful Degradation).
		c.JSON(http.StatusOK, logRes)
		return
	}

	// 5. Mapper Batch để trộn data từ users vào logRes.Items
	// Hàm mapper anh em mình vừa viết ở câu trước
	logRes.Items = mapper.MapStaffToTimeLogsBatch(logRes.Items, users)

	// 6. Trả về kết quả cuối cùng đã được "Enrich" (làm giàu dữ liệu)
	c.JSON(http.StatusOK, logRes)
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
func (h *ShiftHandler) UpdateTimeLog(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req shift.UpdateTimeLogRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	// Nếu status là Invalid (2) thì ManagerNote không được trống
	if req.Status == enum.TimeLogStatus_INVALID && (req.ManagerNote == nil || *req.ManagerNote == "") {
		c.Error(&errormodel.BackendError{
			Status: http.StatusBadRequest,
			Title:  "Thao tác không thành công",
			Detail: "Phải nhập lý do khi từ chối chấm công",
		})
		return
	}

	fmt.Printf("Updating TimeLog %s to Status %d with Note: %v\n", id, req.Status, req.ManagerNote)
	// err := h.shiftGW.UpdateTimeLog(c.Request.Context(), id, req)
	// if err != nil {
	// 	c.Error(err)
	// 	return
	// }

	c.JSON(http.StatusOK, gin.H{})
}
