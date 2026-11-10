package sales

import (
	"api-gateway/config"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/wallet"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"fmt"
	"net/http"
	"net/url"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type WalletHandler struct {
	walletGW sales.IWalletService
	cfg      *config.Config
}

func NewWalletHandler(walletGW sales.IWalletService, cfg *config.Config) *WalletHandler {
	return &WalletHandler{
		walletGW: walletGW,
		cfg:      cfg,
	}
}

// ----------------------------------------------------------------------
// NHÓM API DÀNH CHO END-USER (Tiền tố: /me/wallet)
// ----------------------------------------------------------------------

// GetMyWallet
// @Summary      Get my wallet details
// @Description  Retrieve the wallet information and current balance of the logged-in user.
// @Tags         Wallets
// @Produce      json
// @Security     BearerAuth
// @Success      200  {object} wallet.WalletResponse "Detailed wallet information"
// @Failure      401  {object} map[string]string     "Unauthorized or invalid token"
// @Failure      500  {object} map[string]string     "Internal Server Error"
// @Router       /me/wallet [get]
func (h *WalletHandler) GetMyWallet(c *gin.Context) error {
	ctx := c.Request.Context()

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	res, err := h.walletGW.GetByUserId(ctx, userId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// TopUpMyWallet
// @Summary      Top up wallet balance
// @Description  Generate a VNPay payment link to allow the user to deposit funds into their personal wallet.
// @Tags         Wallets
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     wallet.TopUpWalletRequest true "Top-up details (e.g., amount)"
// @Success      200     {object} map[string]string         "Returns the VNPay URL { paymentUrl: '...' }"
// @Failure      400     {object} map[string]string         "Validation Error (Bad Request)"
// @Failure      401     {object} map[string]string         "Unauthorized"
// @Failure      500     {object} map[string]string         "Internal Server Error or Payment Gateway connection issue"
// @Router       /me/wallet/top-up [post]
func (h *WalletHandler) TopUpMyWallet(c *gin.Context) error {
	var req wallet.TopUpWalletRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	paymentUrl, err := h.walletGW.TopUp(ctx, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"paymentUrl": paymentUrl,
	})
	return nil
}

// ----------------------------------------------------------------------
// NHÓM API DÀNH CHO ADMIN / QUẢN LÝ (Tiền tố: /wallets)
// ----------------------------------------------------------------------

// GetWalletByUserId
// @Summary      Get user wallet details (Admin)
// @Description  Intended for Staff, Manager, or Admin to retrieve wallet information of a specific user by their User ID.
// @Tags         Wallets
// @Produce      json
// @Security     BearerAuth
// @Param        userId  path     string                true "Target User UUID"
// @Success      200     {object} wallet.WalletResponse "Detailed wallet information"
// @Failure      400     {object} map[string]string     "Invalid User UUID format"
// @Failure      401     {object} map[string]string     "Unauthorized"
// @Failure      403     {object} map[string]string     "Forbidden (Insufficient permissions)"
// @Failure      404     {object} map[string]string     "Wallet not found for this user"
// @Router       /wallets/users/{userId} [get]
func (h *WalletHandler) GetWalletByUserId(c *gin.Context) error {
	targetUserIdStr := c.Param("userId")
	targetUserId, err := uuid.Parse(targetUserIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	res, err := h.walletGW.GetByUserId(ctx, targetUserId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateWalletStatus
// @Summary      Update wallet status (Admin)
// @Description  Intended for Administrators to lock (suspend) or unlock a user's wallet.
// @Tags         Wallets
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                           true "Target Wallet UUID"
// @Param        request  body     wallet.UpdateWalletStatusRequest true "New status information"
// @Success      200      {object} map[string]string                "{"message": "Cập nhật trạng thái ví thành công"}"
// @Failure      400      {object} map[string]string                "Validation Error or Invalid Wallet UUID"
// @Failure      401      {object} map[string]string                "Unauthorized"
// @Failure      403      {object} map[string]string                "Forbidden (Insufficient permissions)"
// @Router       /wallets/{id}/status [patch]
func (h *WalletHandler) UpdateWalletStatus(c *gin.Context) error {
	var req wallet.UpdateWalletStatusRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	walletIdStr := c.Param("id")
	walletId, err := uuid.Parse(walletIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	adminId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	err = h.walletGW.UpdateStatus(ctx, adminId, walletId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Cập nhật trạng thái ví thành công"})
	return nil
}

// ----------------------------------------------------------------------
// NHÓM API CALLBACK TỪ ĐỐI TÁC (Public)
// ----------------------------------------------------------------------

// ProcessTopUpVnPayReturn
// @Summary      Handle VNPay Return URL (Wallet Top-up)
// @Description  Callback endpoint for VNPay after a customer completes a wallet top-up payment. Validates the gRPC signature and redirects the user to the Frontend UI with the transaction status.
// @Tags         Wallets
// @Produce      html
// @Param        redirectTo       query string false "Frontend path to redirect to (e.g., /my-wallet)"
// @Param        vnp_Amount       query string false "Transaction Amount (multiplied by 100)"
// @Param        vnp_ResponseCode query string false "VNPay Response Code (00 means success)"
// @Param        vnp_TxnRef       query string false "Internal Transaction Reference"
// @Param        vnp_SecureHash   query string false "Checksum Security Hash"
// @Success      302 "Redirects to the Frontend application"
// @Router       /wallets/vnpay/return [get]
func (h *WalletHandler) ProcessTopUpVnPayReturn(c *gin.Context) error {
	vnpayParams := make(map[string]string)
	redirectTo := ""

	for key, values := range c.Request.URL.Query() {
		if len(values) > 0 {
			if key == "redirectTo" {
				redirectTo = values[0]
			} else if strings.HasPrefix(key, "vnp_") {
				vnpayParams[key] = values[0]
			}
		}
	}

	// Validate redirectTo chống Open Redirect
	if after, ok := strings.CutPrefix(redirectTo, h.cfg.FrontEndOrigin); ok {
		redirectTo = after
	}

	if redirectTo == "" ||
		!strings.HasPrefix(redirectTo, "/") ||
		strings.HasPrefix(redirectTo, "//") ||
		strings.Contains(strings.ToLower(redirectTo), "http://") ||
		strings.Contains(strings.ToLower(redirectTo), "https://") {

		parsedUrl, err := url.Parse(h.cfg.VNPay.WalletClientReturnUrl)
		if err == nil && parsedUrl.Path != "" {
			redirectTo = parsedUrl.Path
		} else {
			redirectTo = "/wallet"
		}
	}

	ctx := c.Request.Context()
	res, err := h.walletGW.TopUpVnPayReturn(ctx, vnpayParams)

	paymentStatus := "failed"
	msg := "Nạp tiền thất bại hoặc giao dịch bị hủy"

	if err == nil && res != nil && res.Success {
		paymentStatus = "success"
		msg = "Nạp tiền thành công"
	}

	// Mã hóa thông báo để gắn lên URL (tránh lỗi font tiếng Việt)
	safeMsg := url.QueryEscape(msg)
	separator := "?"
	if strings.Contains(redirectTo, "?") {
		separator = "&"
	}

	// Build link Frontend: http://localhost:3000/wallet?payment_status=success&message=Nạp+tiền+thành+công
	finalRedirectUrl := fmt.Sprintf("%s%s%spayment_status=%s&message=%s",
		h.cfg.FrontEndOrigin,
		redirectTo,
		separator,
		paymentStatus,
		safeMsg,
	)

	c.Redirect(http.StatusFound, finalRedirectUrl)
	return nil
}
