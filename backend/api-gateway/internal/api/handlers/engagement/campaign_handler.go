package engagement

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/campaign"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type CampaignHandler struct {
	campaignGW engagement.ICampaignService
}

func NewCampaignHandler(
	campaignGW engagement.ICampaignService,
) *CampaignHandler {
	return &CampaignHandler{
		campaignGW: campaignGW,
	}
}

// GetAll
// @Summary      Get all campaigns
// @Description  Retrieve a list of all existing campaigns / promotional programs in the system.
// @Tags         Campaigns
// @Produce      json
// @Success      200 {array}  campaign.CampaignResponse "Danh sách chiến dịch thành công"
// @Failure      500 {object} object "Internal Server Error"
// @Router       /campaigns [get]
func (h *CampaignHandler) GetAll(c *gin.Context) error {
	ctx := c.Request.Context()

	res, err := h.campaignGW.GetAll(ctx)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetActive
// @Summary      Get active campaigns
// @Description  Retrieve a list of currently active campaigns / promotional programs.
// @Tags         Campaigns
// @Produce      json
// @Success      200 {array}  campaign.CampaignResponse "Danh sách chiến dịch"
// @Failure      500 {object} object "Internal Server Error"
// @Router       /campaigns/active [get]
func (h *CampaignHandler) GetActive(c *gin.Context) error {
	ctx := c.Request.Context()

	res, err := h.campaignGW.GetActive(ctx)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetById
// @Summary      Get campaign details
// @Description  Retrieve detailed information of a specific campaign by its ID.
// @Tags         Campaigns
// @Produce      json
// @Param        id   path      string true "Campaign ID"
// @Success      200  {object}  campaign.CampaignResponse "Chi tiết chiến dịch"
// @Failure      400  {object}  object "Invalid ID (Bad Request)"
// @Failure      404  {object}  object "Campaign Not Found"
// @Failure      500  {object}  object "Internal Server Error"
// @Router       /campaigns/{id} [get]
func (h *CampaignHandler) GetById(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	data, err := h.campaignGW.GetById(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, data)
	return nil
}

// Create
// @Summary      Create a new campaign
// @Description  Create a new campaign or voucher hunting event.
// @Tags         Campaigns
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body      campaign.CreateCampaignRequest true "Campaign initialization data"
// @Success      201  {object}  map[string]string "{"message": "Tạo chiến dịch thành công"}"
// @Failure      400  {object}  object "Validation Error (Bad Request)"
// @Failure      401  {object}  object "Unauthorized"
// @Failure      500  {object}  object "Internal Server Error"
// @Router       /campaigns [post]
func (h *CampaignHandler) Create(c *gin.Context) error {
	var req campaign.CreateCampaignRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	if err := h.campaignGW.Create(ctx, &req); err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Tạo chiến dịch thành công"})
	return nil
}

// UpdateActive
// @Summary      Update campaign active status
// @Description  Enable or disable the active status of a specific campaign.
// @Tags         Campaigns
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path      string true "Campaign ID"
// @Param        body body      campaign.UpdateActiveStatusRequest true "Status update data"
// @Success      200  {object}  map[string]string "{"message": "Cập nhật trạng thái chiến dịch thành công"}"
// @Failure      400  {object}  object "Invalid input data or ID"
// @Failure      401  {object}  object "Unauthorized"
// @Failure      404  {object}  object "Campaign Not Found"
// @Failure      500  {object}  object "Internal Server Error"
// @Router       /campaigns/{id}/active [patch]
func (h *CampaignHandler) UpdateActive(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req campaign.UpdateActiveStatusRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	if err := h.campaignGW.UpdateActive(ctx, id, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Cập nhật trạng thái chiến dịch thành công"})
	return nil
}

// RedeemVoucher
// @Summary      Redeem voucher from campaign
// @Description  User redeems points for a voucher or claims a free voucher from a specific campaign.
// @Tags         Campaigns
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        voucherId   path      string  true  "Voucher ID"
// @Success      200         {object}  map[string]string "{"message": "Đổi thành công!"}"
// @Failure      400         {object}  object "Invalid ID or bad request data"
// @Failure      401         {object}  object "Unauthorized"
// @Failure      403         {object}  object "Insufficient points or Forbidden"
// @Failure      404         {object}  object "Campaign or voucher not found"
// @Failure      500         {object}  object "Internal Server Error"
// @Router       /campaigns/vouchers/{voucherId}/redeem [post]
func (h *CampaignHandler) RedeemVoucher(c *gin.Context) error {
	voucherId, err := uuid.Parse(c.Param("voucherId"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()

	if err := h.campaignGW.RedeemVoucher(ctx, userId, voucherId); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "Đổi thành công!",
	})
	return nil
}
