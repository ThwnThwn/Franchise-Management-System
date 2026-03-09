package engagement

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/voucher"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type VoucherHandler struct {
	voucherGW engagement.IVoucherService
	loyaltyGW engagement.ILoyaltyAccountService
}

func NewVoucherHandler(
	voucherGW engagement.IVoucherService,
	loyaltyGW engagement.ILoyaltyAccountService,
) *VoucherHandler {
	return &VoucherHandler{
		voucherGW: voucherGW,
		loyaltyGW: loyaltyGW,
	}
}

// GetAllVouchers
// @Summary      Get All Vouchers
// @Description  Retrieve a list of all vouchers available in the system.
// @Tags         Vouchers
// @Accept       json
// @Produce      json
// @Success      200    {array}   voucher.VoucherResponse "List of vouchers"
// @Router       /vouchers [get]
func (h *VoucherHandler) GetAllVouchers(c *gin.Context) error {
	ctx := c.Request.Context()

	res, err := h.voucherGW.GetAll(ctx)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetVoucherByCode
// @Summary      Get Voucher by Code
// @Description  Retrieve voucher details using its unique string code (e.g., 'SALE50').
// @Tags         Vouchers
// @Accept       json
// @Produce      json
// @Param        code   path      string  true  "Voucher Code"
// @Success      200    {object}  voucher.VoucherResponse "Voucher details"
// @Failure      404    {object}  map[string]string       "Voucher not found"
// @Router       /vouchers/code/{code} [get]
func (h *VoucherHandler) GetVoucherByCode(c *gin.Context) error {
	code := c.Param("code")
	if code == "" {
		return errorcode.NewBadRequest("Mã voucher không được để trống")
	}

	ctx := c.Request.Context()

	res, err := h.voucherGW.GetByCode(ctx, code)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetMyVouchers
// @Summary      Get My Vouchers
// @Description  Retrieve a list of all vouchers available in my account.
// @Tags         Vouchers
// @Accept       json
// @Produce      json
// @Success      200    {array}   voucher.VoucherResponse "List of vouchers"
// @Router       /me/vouchers [get]
func (h *VoucherHandler) GetMyVouchers(c *gin.Context) error {
	ctx := c.Request.Context()
	res, err := h.voucherGW.GetMyVouchers(ctx)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// CreateVoucher
// @Summary      Create New Voucher
// @Description  Create a new discount voucher with items and rules.
// @Tags         Vouchers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body      voucher.CreateVoucherRequest  true  "Voucher details"
// @Success      201    {object}  map[string]interface{}      "Success"
// @Router       /vouchers [post]
func (h *VoucherHandler) CreateVoucher(c *gin.Context) error {
	var req voucher.CreateVoucherRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err := h.voucherGW.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Tạo voucher thành công"})
	return nil
}

// UpdateVoucher
// @Summary      Update Voucher
// @Description  Update details of an existing voucher. Fields are optional (patch-like behavior).
// @Tags         Vouchers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path      string                        true  "Voucher UUID"
// @Param        request body      voucher.UpdateVoucherRequest  true  "Updated fields"
// @Success      200    {object}  map[string]interface{}      "Success"
// @Router       /vouchers/{id} [patch]
func (h *VoucherHandler) UpdateVoucher(c *gin.Context) error {
	id := c.Param("id")
	voucherId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req voucher.UpdateVoucherRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err = h.voucherGW.Update(ctx, voucherId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Cập nhật voucher thành công"})
	return nil
}

// UpdateVoucherStatus
// @Summary      Update Voucher Status
// @Description  Activate or deactivate a voucher.
// @Tags         Vouchers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path      string                              true  "Voucher UUID"
// @Param        request body      voucher.UpdateVoucherStatusRequest  true  "New status"
// @Success      200    {object}  map[string]interface{}            "Success"
// @Router       /vouchers/{id}/status [patch]
func (h *VoucherHandler) UpdateVoucherStatus(c *gin.Context) error {
	id := c.Param("id")
	voucherId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req voucher.UpdateVoucherStatusRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err = h.voucherGW.UpdateStatus(ctx, voucherId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Cập nhật trạng thái voucher thành công"})
	return nil
}

// DeleteVoucher
// @Summary      Delete Voucher
// @Description  Permanently remove a voucher from the system.
// @Tags         Vouchers
// @Security     BearerAuth
// @Param        id      path      string  true  "Voucher UUID"
// @Success      200    {object}  map[string]interface{}  "Success"
// @Router       /vouchers/{id} [delete]
func (h *VoucherHandler) DeleteVoucher(c *gin.Context) error {
	id := c.Param("id")
	voucherId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	err = h.voucherGW.Delete(ctx, voucherId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Xóa voucher thành công"})
	return nil
}

// RedeemVoucher
// @Summary      Redeem Points for Voucher
// @Description  Allows the authenticated user to redeem their accumulated points to claim a specific voucher.
// @Tags         Vouchers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string  true  "Voucher UUID"
// @Success      200  {object}  map[string]interface{}  "Redemption successful"
// @Failure      400  {object}  map[string]string       "Invalid Voucher ID format"
// @Failure      401  {object}  map[string]string       "Unauthorized - Valid token required"
// @Router       /me/vouchers/{id}/redeem [post]
func (h *VoucherHandler) RedeemVoucher(c *gin.Context) error {
	id := c.Param("id")
	voucherId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	err = h.loyaltyGW.RedeemVoucher(ctx, voucherId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}
