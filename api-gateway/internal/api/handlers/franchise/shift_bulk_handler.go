package franchise

import (
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/validation"
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftBulkHandler struct {
	shiftGW franchise.IShiftService
}

func NewShiftBulkHandler(shiftGW franchise.IShiftService) *ShiftBulkHandler {
	return &ShiftBulkHandler{shiftGW: shiftGW}
}

// CreateManual
// @Summary      Tạo ca làm việc thủ công hàng loạt
// @Description  Tạo nhiều ca làm việc cùng lúc dựa trên khung giờ và các ngày trong tuần được chọn.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Param        body body shift.BulkCreateManualRequest true "Dữ liệu tạo hàng loạt"
// @Router       /shifts/bulk-manual [post]
func (h *ShiftBulkHandler) CreateManual(c *gin.Context) {
	var req shift.BulkCreateManualRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(req)
	// err := h.shiftGW.BulkCreateManual(c.Request.Context(), req)

	c.JSON(http.StatusCreated, gin.H{})
}

// CreateFromTemplate
// @Summary      Tạo ca làm việc từ mẫu hàng loạt
// @Description  Tạo hàng loạt ca làm việc trong một khoảng thời gian dựa trên các Shift Template có sẵn.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Param        body body shift.BulkCreateFromTemplateRequest true "Dữ liệu tạo từ mẫu"
// @Router       /shifts/bulk-template [post]
func (h *ShiftBulkHandler) CreateFromTemplate(c *gin.Context) {
	var req shift.BulkCreateFromTemplateRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(req)
	// err := h.shiftGW.BulkCreateFromTemplate(c.Request.Context(), req)

	c.JSON(http.StatusCreated, gin.H{})
}

// Publish
// @Summary      Công khai ca làm việc hàng loạt
// @Description  Chuyển trạng thái hàng loạt ca làm từ Draft sang Published để nhân viên có thể thấy lịch.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Param        body body []string true "Danh sách ID các ca làm cần công khai"
// @Router       /shifts/bulk-publish [put]
func (h *ShiftBulkHandler) Publish(c *gin.Context) {
	var ids []uuid.UUID
	if err := c.ShouldBindJSON(&ids); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(ids)
	// err := h.shiftGW.BulkPublish(c.Request.Context(), ids)

	c.JSON(http.StatusOK, gin.H{})
}

// Delete
// @Summary      Xóa hàng loạt ca làm việc
// @Description  Xóa hàng loạt các ca làm việc (Chỉ áp dụng cho các ca đang ở trạng thái Nháp).
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Param        body body []string true "Danh sách ID các ca làm cần xóa"
// @Router       /shifts/bulk-delete [delete]
func (h *ShiftBulkHandler) Delete(c *gin.Context) {
	var ids []uuid.UUID
	if err := c.ShouldBindJSON(&ids); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(ids)
	// err := h.shiftGW.BulkDelete(c.Request.Context(), ids)

	c.JSON(http.StatusOK, gin.H{})
}
