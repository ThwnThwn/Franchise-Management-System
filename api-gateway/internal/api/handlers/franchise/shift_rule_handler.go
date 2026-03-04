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

type ShiftRuleHandler struct {
	shiftGW franchise.IShiftRuleService
}

func NewShiftRuleHandler(shiftGW franchise.IShiftRuleService) *ShiftRuleHandler {
	return &ShiftRuleHandler{shiftGW: shiftGW}
}

// GetSettings
// @Summary      Lấy cấu hình quy định ca làm
// @Description  Lấy thông tin cấu hình luật ca làm việc (Global và theo Store) để hiển thị trên trang cài đặt.
// @Tags         Shifts: Rule
// @Security     BearerAuth
// @Param        storeId query string false "ID cửa hàng (Nếu muốn lọc theo store cụ thể)"
// @Success      200 {object} shift.ShiftRuleSettingsResponse "Dữ liệu cấu hình"
// @Router       /shift-rules [get]
func (h *ShiftRuleHandler) GetSettings(c *gin.Context) error {
	// FE gửi params: { storeId } qua Query String
	var params struct {
		StoreID string `form:"storeId"`
	}

	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	res, err := h.shiftGW.GetSettings(c.Request.Context(), params.StoreID)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Save
// @Summary      Lưu cấu hình quy định ca làm
// @Description  Cập nhật hoặc tạo mới các quy định về ca làm việc (Giờ tối đa, thời gian nghỉ...).
// @Tags         Shifts: Rule
// @Security     BearerAuth
// @Param        payload body shift.SaveShiftRuleRequest true "Dữ liệu quy định cần lưu"
// @Router       /shift-rules [put]
func (h *ShiftRuleHandler) Save(c *gin.Context) error {
	var req shift.SaveShiftRuleRequest
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

	contextutils.ApplyStoreBoundaries(c, &req.StoreID)

	err = h.shiftGW.Save(ctx, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}
