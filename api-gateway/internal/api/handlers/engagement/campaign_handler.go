package engagement

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/campaign"
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
// @Summary      Lấy danh sách chiến dịch
// @Description  Lấy toàn bộ danh sách các chiến dịch / chương trình khuyến mãi hiện có trên hệ thống.
// @Tags         Campaigns
// @Produce      json
// @Success      200 {array}   campaign.CampaignResponse "Danh sách chiến dịch thành công"
// @Failure      500 {object}  object "Lỗi hệ thống (Internal Server Error)"
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

// GetById
// @Summary      Lấy chi tiết chiến dịch
// @Description  Lấy thông tin chi tiết của một chiến dịch dựa vào ID.
// @Tags         Campaigns
// @Produce      json
// @Param        id   path      string true "Campaign ID (Mã chiến dịch)"
// @Success      200  {object}  campaign.CampaignResponse "Chi tiết chiến dịch"
// @Failure      400  {object}  object "ID không hợp lệ (Bad Request)"
// @Failure      404  {object}  object "Không tìm thấy chiến dịch (Not Found)"
// @Failure      500  {object}  object "Lỗi hệ thống (Internal Server Error)"
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
// @Summary      Tạo chiến dịch mới
// @Description  Tạo một chiến dịch / đợt săn voucher mới.
// @Tags         Campaigns
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        body body      campaign.CreateCampaignRequest true "Dữ liệu khởi tạo chiến dịch"
// @Success      201  {object}  map[string]string "{"message": "Tạo chiến dịch thành công"}"
// @Failure      400  {object}  object "Lỗi dữ liệu đầu vào (Validation Error)"
// @Failure      401  {object}  object "Chưa xác thực (Unauthorized)"
// @Failure      500  {object}  object "Lỗi hệ thống (Internal Server Error)"
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
// @Summary      Cập nhật trạng thái hoạt động của chiến dịch
// @Description  Bật hoặc tắt trạng thái hoạt động (Active/Inactive) của một chiến dịch cụ thể.
// @Tags         Campaigns
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path      string true "Campaign ID (Mã chiến dịch)"
// @Param        body body      campaign.UpdateActiveStatusRequest true "Dữ liệu cập nhật trạng thái"
// @Success      200  {object}  map[string]string "{"message": "Cập nhật trạng thái chiến dịch thành công"}"
// @Failure      400  {object}  object "Lỗi dữ liệu đầu vào hoặc ID không hợp lệ"
// @Failure      401  {object}  object "Chưa xác thực (Unauthorized)"
// @Failure      404  {object}  object "Không tìm thấy chiến dịch (Not Found)"
// @Failure      500  {object}  object "Lỗi hệ thống (Internal Server Error)"
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
