package franchise

import (
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/validation"
	"fmt"
	"net/http"

	"github.com/gin-gonic/gin"
)

type ShiftRuleHandler struct {
	shiftGW franchise.IShiftService
}

func NewShiftRuleHandler(shiftGW franchise.IShiftService) *ShiftRuleHandler {
	return &ShiftRuleHandler{shiftGW: shiftGW}
}

// GetSettings
// @Summary      Lấy cấu hình quy định ca làm
// @Description  Lấy thông tin cấu hình luật ca làm việc (Global và theo Store) để hiển thị trên trang cài đặt.
// @Tags         Quản lý Ca làm
// @Security     BearerAuth
// @Param        storeId query string false "ID cửa hàng (Nếu muốn lọc theo store cụ thể)"
// @Success      200 {object} shift.ShiftRuleSettingsResponse "Dữ liệu cấu hình"
// @Router       /shift-rules [get]
func (h *ShiftRuleHandler) GetSettings(c *gin.Context) {
	// FE gửi params: { storeId } qua Query String
	var params struct {
		StoreID string `form:"storeId"`
	}

	if err := c.ShouldBindQuery(&params); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(params.StoreID)
	// res, err := h.shiftGW.GetRuleSettings(c.Request.Context(), params.StoreID)

	// Mock response dựa theo ShiftRuleSettingsResponse đã định nghĩa
	res := gin.H{}
	c.JSON(http.StatusOK, res)
}

// Save
// @Summary      Lưu cấu hình quy định ca làm
// @Description  Cập nhật hoặc tạo mới các quy định về ca làm việc (Giờ tối đa, thời gian nghỉ...).
// @Tags         Quản lý Ca làm
// @Security     BearerAuth
// @Param        payload body shift.SaveShiftRuleRequest true "Dữ liệu quy định cần lưu"
// @Router       /shift-rules [put]
func (h *ShiftRuleHandler) Save(c *gin.Context) {
	var req shift.SaveShiftRuleRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	fmt.Println(req)
	// err := h.shiftGW.SaveRule(c.Request.Context(), req)

	c.JSON(http.StatusOK, gin.H{})
}
