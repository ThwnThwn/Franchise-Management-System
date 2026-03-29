package franchise

import (
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
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
// @Summary      Get shift rule settings
// @Description  Retrieve shift rule configurations (Global and Store-specific) to display on the settings page.
// @Tags         Shifts: Rule
// @Security     BearerAuth
// @Produce      json
// @Param        storeId query  string false "Store ID (Optional, to filter by a specific store)"
// @Success      200     {object} shift.ShiftRuleSettingsResponse "Configuration data"
// @Failure      400     {object} object "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
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
// @Summary      Save shift rule settings
// @Description  Create or update shift rules and constraints (e.g., maximum working hours, required break times).
// @Tags         Shifts: Rule
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        payload body   shift.SaveShiftRuleRequest true "Rule configuration data to save"
// @Success      200     {object} map[string]interface{} "Successfully saved rule settings"
// @Failure      400     {object} object "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /shift-rules [put]
func (h *ShiftRuleHandler) Save(c *gin.Context) error {
	var req shift.SaveShiftRuleRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	err = h.shiftGW.Save(ctx, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}
