package franchise

import (
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
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
// @Summary      Bulk create manual shifts
// @Description  Create multiple shifts simultaneously based on selected time frames and days of the week.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body shift.BulkCreateManualRequest true "Bulk creation data"
// @Success      201  {object} map[string]interface{} "Successfully created shifts"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/bulk-manual [post]
func (h *ShiftBulkHandler) CreateManual(c *gin.Context) error {
	var req shift.BulkCreateManualRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
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
// @Summary      Bulk create shifts from template
// @Description  Create multiple shifts over a specified period based on existing Shift Templates.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body shift.BulkCreateFromTemplateRequest true "Template creation data"
// @Success      201  {object} map[string]interface{} "Successfully created shifts from template"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/bulk-template [post]
func (h *ShiftBulkHandler) CreateFromTemplate(c *gin.Context) error {
	var req shift.BulkCreateFromTemplateRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
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
// @Summary      Bulk publish shifts
// @Description  Change the status of multiple shifts from Draft to Published, making them visible to employees on the schedule.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body []string true "List of shift IDs to publish"
// @Success      200  {object} map[string]interface{} "Successfully published shifts"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/bulk-publish [put]
func (h *ShiftBulkHandler) Publish(c *gin.Context) error {
	var ids []string
	if err := c.ShouldBindJSON(&ids); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err := h.shiftGW.BulkPublish(ctx, ids)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Delete
// @Summary      Bulk delete shifts
// @Description  Delete multiple shifts simultaneously (Only applicable for shifts currently in Draft status).
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body []string true "List of shift IDs to delete"
// @Success      200  {object} map[string]interface{} "Successfully deleted shifts"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/bulk-delete [delete]
func (h *ShiftBulkHandler) Delete(c *gin.Context) error {
	var ids []string
	if err := c.ShouldBindJSON(&ids); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err := h.shiftGW.BulkDelete(ctx, ids)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Open
// @Summary      Bulk open shifts to Shift Pool
// @Description  Change the status of multiple shifts from Draft to Open, allowing employees to apply for them in the Shift Pool. StoreId is automatically extracted from the Manager's Token.
// @Tags         Shifts: Bulk action
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body []string true "List of shift IDs to open"
// @Success      200  {object} map[string]interface{} "Returns the count of successfully opened shifts"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shifts/bulk-open [put]
func (h *ShiftBulkHandler) Open(c *gin.Context) error {
	var ids []string
	if err := c.ShouldBindJSON(&ids); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
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
