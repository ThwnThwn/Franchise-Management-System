package engagement

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/membershiptier"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type MembershipTierHandler struct {
	tierGW engagement.IMembershipTierService
}

func NewMembershipTierHandler(tierGW engagement.IMembershipTierService) *MembershipTierHandler {
	return &MembershipTierHandler{
		tierGW: tierGW,
	}
}

// GetAllTiers
// @Summary      Get All Membership Tiers
// @Description  Retrieve a list of all available membership tiers
// @Tags         Membership Tiers
// @Accept       json
// @Produce      json
// @Success      200    {array}   membershiptier.MembershipTierResponse "List of tiers"
// @Router       /membership-tiers [get]
func (h *MembershipTierHandler) GetAllTiers(c *gin.Context) error {
	ctx := c.Request.Context()
	res, err := h.tierGW.GetAll(ctx)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// CreateTier
// @Summary      Create Membership Tier
// @Description  Define a new membership tier with spending requirements and benefits.
// @Tags         Membership Tiers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body      membershiptier.CreateMembershipTierRequest  true  "Tier details"
// @Success      201    {object}  map[string]interface{}                    "Success"
// @Failure      400    {object}  map[string]string                         "Validation error"
// @Router       /membership-tiers [post]
func (h *MembershipTierHandler) CreateTier(c *gin.Context) error {
	var req membershiptier.CreateMembershipTierRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	err = h.tierGW.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Tạo hạng thành viên thành công"})
	return nil
}

// UpdateTier
// @Summary      Update Membership Tier
// @Description  Update benefits or requirements for an existing tier.
// @Tags         Membership Tiers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path      string                                     true  "Tier UUID"
// @Param        request body      membershiptier.UpdateMembershipTierRequest  true  "Updated details"
// @Success      200    {object}  map[string]interface{}                    "Success"
// @Failure      400    {object}  map[string]string                         "Invalid ID or validation error"
// @Router       /membership-tiers/{id} [patch]
func (h *MembershipTierHandler) UpdateTier(c *gin.Context) error {
	id := c.Param("id")
	tierId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req membershiptier.UpdateMembershipTierRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	err = h.tierGW.Update(ctx, tierId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Cập nhật hạng thành viên thành công"})
	return nil
}

// DeleteTier
// @Summary      Delete Membership Tier
// @Description  Remove a membership tier from the system.
// @Tags         Membership Tiers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path      string  true  "Tier UUID"
// @Success      200    {object}  map[string]interface{}  "Success"
// @Failure      400    {object}  map[string]string       "Invalid ID format"
// @Router       /membership-tiers/{id} [delete]
func (h *MembershipTierHandler) DeleteTier(c *gin.Context) error {
	id := c.Param("id")
	tierId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	err = h.tierGW.Delete(ctx, tierId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Xóa hạng thành viên thành công"})
	return nil
}
