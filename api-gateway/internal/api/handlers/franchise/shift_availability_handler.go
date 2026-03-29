package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftAvailabilityHandler struct {
	shiftGW franchise.IShiftAvailabilityService
}

func NewShiftAvailabilityHandler(shiftGW franchise.IShiftAvailabilityService) *ShiftAvailabilityHandler {
	return &ShiftAvailabilityHandler{shiftGW: shiftGW}
}

// ===============================
// STAFF SIDE (Của tôi)
// ===============================

// GetMyHistory
// @Summary      Get my shift availability history
// @Description  Retrieve the paginated list of registered availability/unavailability for the current employee.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Produce      json
// @Param        query query shift.GetAvailabilityParams false "Search filters"
// @Success      200 {object} common.PaginationResult[shift.ShiftAvailabilityResponse] "Successful retrieval"
// @Failure      400 {object} object "Bad Request"
// @Failure      401 {object} object "Unauthorized"
// @Failure      500 {object} object "Internal Server Error"
// @Router       /me/shift-availabilities [get]
func (h *ShiftAvailabilityHandler) GetMyHistory(c *gin.Context) error {
	userID, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	var params shift.GetAvailabilityParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	params.UserId = stringutils.ToPtr(userID.String())
	res, err := h.shiftGW.GetAll(c.Request.Context(), &params)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Create
// @Summary      Register new shift availability
// @Description  Submit a request for available or unavailable time to assist managers in scheduling.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body shift.CreateShiftAvailabilityRequest true "Availability registration data"
// @Success      201 {object} map[string]interface{} "Successfully created"
// @Failure      400 {object} object "Validation Error (Bad Request)"
// @Failure      401 {object} object "Unauthorized"
// @Failure      500 {object} object "Internal Server Error"
// @Router       /shift-availabilities [post]
func (h *ShiftAvailabilityHandler) Create(c *gin.Context) error {
	userID, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	var req shift.CreateShiftAvailabilityRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.shiftGW.Create(ctx, userID, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{})
	return nil
}

// Update
// @Summary      Update shift availability
// @Description  Edit a submitted availability/unavailability request by its ID.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path string true "Availability ID"
// @Param        body body shift.UpdateShiftAvailabilityRequest true "Update data"
// @Success      200 {object} map[string]interface{} "Successfully updated"
// @Failure      400 {object} object "Invalid ID or Bad Request"
// @Failure      401 {object} object "Unauthorized"
// @Failure      500 {object} object "Internal Server Error"
// @Router       /shift-availabilities/{id} [patch]
func (h *ShiftAvailabilityHandler) Update(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.UpdateShiftAvailabilityRequest
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

// Delete
// @Summary      Delete shift availability
// @Description  Remove a previously submitted availability request.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Produce      json
// @Param        id path string true "Availability ID"
// @Success      200 {object} map[string]interface{} "Successfully deleted"
// @Failure      400 {object} object "Invalid ID"
// @Failure      401 {object} object "Unauthorized"
// @Failure      500 {object} object "Internal Server Error"
// @Router       /shift-availabilities/{id} [delete]
func (h *ShiftAvailabilityHandler) Delete(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	err = h.shiftGW.Delete(ctx, id, userId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// ===============================
// MANAGER SIDE (Quản lý)
// ===============================

// GetAll
// @Summary      Manage all shift availabilities (Manager)
// @Description  Managers view the complete list of employee availabilities to facilitate shift scheduling.
// @Tags         Shifts: Availabilities
// @Security     BearerAuth
// @Produce      json
// @Param        query query shift.GetAvailabilityParams false "Search filters (Store, User, Date...)"
// @Success      200 {object} common.PaginationResult[shift.ShiftAvailabilityResponse] "Successful retrieval"
// @Failure      400 {object} object "Bad Request"
// @Failure      401 {object} object "Unauthorized"
// @Failure      500 {object} object "Internal Server Error"
// @Router       /shift-availabilities [get]
func (h *ShiftAvailabilityHandler) GetAll(c *gin.Context) error {
	var params shift.GetAvailabilityParams
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
