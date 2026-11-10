package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftTemplateHandler struct {
	shiftGW franchise.IShiftTemplateService
}

func NewShiftTemplateHandler(shiftGW franchise.IShiftTemplateService) *ShiftTemplateHandler {
	return &ShiftTemplateHandler{shiftGW: shiftGW}
}

// GetAll
// @Summary      Get all shift templates
// @Description  Retrieve a complete list of shift templates (e.g., Morning, Afternoon, Night) for a specific store.
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Produce      json
// @Param        query query  shift.GetShiftTemplatesParams false "Shift template search filters"
// @Success      200   {array}  shift.ShiftTemplateResponse "List of shift templates"
// @Failure      400   {object} object "Validation Error (Bad Request)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /shift-templates [get]
func (h *ShiftTemplateHandler) GetAll(c *gin.Context) error {
	var params shift.GetShiftTemplatesParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	res, err := h.shiftGW.GetAll(c.Request.Context(), &params)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetByID
// @Summary      Get shift template details
// @Description  Retrieve the detailed configuration of a specific shift template by its ID.
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Produce      json
// @Param        id   path   string true "Template ID"
// @Success      200  {object} shift.ShiftTemplateResponse "Template details"
// @Failure      400  {object} object "Invalid ID (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      404  {object} object "Template Not Found"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shift-templates/{id} [get]
func (h *ShiftTemplateHandler) GetByID(c *gin.Context) error {
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

// Create
// @Summary      Create a new shift template
// @Description  Define a new standard shift timeframe (including code, name, and start/end times).
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body   shift.CreateShiftTemplateRequest true "Shift template data"
// @Success      201  {object} map[string]interface{} "Successfully created template"
// @Failure      400  {object} object "Validation Error (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shift-templates [post]
func (h *ShiftTemplateHandler) Create(c *gin.Context) error {
	var req shift.CreateShiftTemplateRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	err := h.shiftGW.Create(c.Request.Context(), &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{})
	return nil
}

// Update
// @Summary      Update a shift template
// @Description  Modify the timeframe configuration or the name of an existing shift template.
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path   string true "Template ID"
// @Param        body body   shift.UpdateShiftTemplateRequest true "Update data"
// @Success      200  {object} map[string]interface{} "Successfully updated template"
// @Failure      400  {object} object "Validation Error or Invalid ID"
// @Failure      401  {object} object "Unauthorized"
// @Failure      404  {object} object "Template Not Found"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shift-templates/{id} [patch]
func (h *ShiftTemplateHandler) Update(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.UpdateShiftTemplateRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	err = h.shiftGW.Update(c.Request.Context(), id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Delete
// @Summary      Delete a shift template
// @Description  Completely remove a shift template from the system.
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Produce      json
// @Param        id   path   string true "Template ID"
// @Success      200  {object} map[string]interface{} "Successfully deleted template"
// @Failure      400  {object} object "Invalid ID (Bad Request)"
// @Failure      401  {object} object "Unauthorized"
// @Failure      404  {object} object "Template Not Found"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /shift-templates/{id} [delete]
func (h *ShiftTemplateHandler) Delete(c *gin.Context) error {
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
