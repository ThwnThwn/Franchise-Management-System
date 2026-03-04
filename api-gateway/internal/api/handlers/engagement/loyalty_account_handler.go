package engagement

import (
	"api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/loyalty"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
)

type LoyaltyHandler struct {
	loyaltyGW engagement.ILoyaltyAccountService
}

func NewLoyaltyHandler(loyaltyGW engagement.ILoyaltyAccountService) *LoyaltyHandler {
	return &LoyaltyHandler{
		loyaltyGW: loyaltyGW,
	}
}

// GetLoyaltyAccounts
// @Summary      Search Loyalty Accounts
// @Description  Retrieve loyalty accounts by user phone or email.
// @Tags         Loyalty
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     loyalty.GetByUserPhoneOrEmailRequest  true  "Search criteria (phone or email)"
// @Success      200    {array}   loyalty.LoyaltyAccountResponse "List of loyalty accounts"
// @Failure      400    {object}  map[string]string              "Validation error"
// @Failure      401    {object}  map[string]string              "Unauthorized"
// @Router       /loyalty/accounts [get]
func (h *LoyaltyHandler) GetLoyaltyAccounts(c *gin.Context) error {
	var req loyalty.GetByUserPhoneOrEmailRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.loyaltyGW.GetByUserPhoneOrEmail(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetLoyaltyReport
// @Summary      Get Loyalty Report
// @Description  Retrieve statistics about loyalty tiers, point distribution, and member growth.
// @Tags         Loyalty
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     loyalty.GetLoyaltyReportRequest  false  "Filter params (fromDate, toDate)"
// @Success      200    {object}  loyalty.LoyaltyReportResponse   "Loyalty report data"
// @Failure      401    {object}  map[string]string               "Unauthorized"
// @Router       /loyalty/reports [get]
func (h *LoyaltyHandler) GetLoyaltyReport(c *gin.Context) error {
	var req loyalty.GetLoyaltyReportRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.loyaltyGW.GetLoyaltyReport(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
