package franchise

import (
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
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
func (h *ShiftBulkHandler) CreateManual(c *gin.Context) error {
	var req shift.BulkCreateManualRequest
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

	if err := h.shiftGW.BulkCreateManual(ctx, userId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{})
	return nil
}

// CreateFromTemplate
// @Summary      Tạo ca làm việc từ mẫu hàng loạt
// @Description  Tạo hàng loạt ca làm việc trong một khoảng thời gian dựa trên các Shift Template có sẵn.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Param        body body shift.BulkCreateFromTemplateRequest true "Dữ liệu tạo từ mẫu"
// @Router       /shifts/bulk-template [post]
func (h *ShiftBulkHandler) CreateFromTemplate(c *gin.Context) error {
	var req shift.BulkCreateFromTemplateRequest
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
	err = h.shiftGW.BulkCreateFromTemplate(ctx, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{})
	return nil
}

// Publish
// @Summary      Công khai ca làm việc hàng loạt
// @Description  Chuyển trạng thái hàng loạt ca làm từ Draft sang Published để nhân viên có thể thấy lịch.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Param        body body []string true "Danh sách ID các ca làm cần công khai"
// @Router       /shifts/bulk-publish [put]
func (h *ShiftBulkHandler) Publish(c *gin.Context) error {
	var ids []string
	if err := c.ShouldBindJSON(&ids); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	err = h.shiftGW.BulkPublish(ctx, ids)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Delete
// @Summary      Xóa hàng loạt ca làm việc
// @Description  Xóa hàng loạt các ca làm việc (Chỉ áp dụng cho các ca đang ở trạng thái Nháp).
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Param        body body []string true "Danh sách ID các ca làm cần xóa"
// @Router       /shifts/bulk-delete [delete]
func (h *ShiftBulkHandler) Delete(c *gin.Context) error {
	var ids []string
	if err := c.ShouldBindJSON(&ids); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	err = h.shiftGW.BulkDelete(ctx, ids)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// BulkOpen
// @Summary      Mở hàng loạt ca ra Chợ Ca (Shift Pool)
// @Description  Chuyển trạng thái hàng loạt ca làm việc từ Draft sang Open để nhân viên có thể đăng ký nhận. StoreId được lấy tự động từ Token của Manager.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Param        body body []string true "Danh sách ID các ca làm cần mở"
// @Router       /shifts/bulk-open [put]
func (h *ShiftBulkHandler) Open(c *gin.Context) error {
	var ids []string
	if err := c.ShouldBindJSON(&ids); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	storeId, err := contextutils.GetStoreId(c)
	if err != nil {
		return err
	}

	openedCount, err := h.shiftGW.BulkOpen(ctx, storeId, ids)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"openedCount": openedCount,
	})
	return nil
}
