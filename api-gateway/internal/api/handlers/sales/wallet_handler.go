package sales

import (
	"api-gateway/config"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/wallet"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
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
// @Summary      Xem ví cá nhân
// @Description  Lấy thông tin ví và số dư hiện tại của người dùng đang đăng nhập.
// @Tags         Wallets
// @Produce      json
// @Security     BearerAuth
// @Success      200  {object}  wallet.WalletResponse "Thông tin chi tiết của ví"
// @Failure      401  {object}  map[string]string     "Chưa đăng nhập hoặc token không hợp lệ"
// @Failure      500  {object}  map[string]string     "Lỗi hệ thống nội bộ"
// @Router       /me/wallet [get]
func (h *WalletHandler) GetMyWallet(c *gin.Context) error {
	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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
// @Summary      Nạp tiền vào ví
// @Description  Tạo link thanh toán qua cổng VNPay để người dùng nạp tiền vào ví cá nhân.
// @Tags         Wallets
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     wallet.TopUpWalletRequest true "Thông tin nạp tiền (VD: số tiền)"
// @Success      200     {object} map[string]string         "Trả về URL để redirect sang VNPay { paymentUrl: '...' }"
// @Failure      400     {object} map[string]string         "Dữ liệu đầu vào không hợp lệ"
// @Failure      401     {object} map[string]string         "Chưa đăng nhập"
// @Failure      500     {object} map[string]string         "Lỗi hệ thống hoặc lỗi kết nối cổng thanh toán"
// @Router       /me/wallet/top-up [post]
func (h *WalletHandler) TopUpMyWallet(c *gin.Context) error {
	var req wallet.TopUpWalletRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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
// @Summary      Xem ví của người dùng
// @Description  Dành cho Staff, Manager, Admin lấy thông tin ví của một user bất kỳ thông qua User ID.
// @Tags         Wallets
// @Produce      json
// @Security     BearerAuth
// @Param        userId  path     string                true "UUID của người dùng cần xem"
// @Success      200     {object} wallet.WalletResponse "Thông tin chi tiết của ví"
// @Failure      400     {object} map[string]string     "ID người dùng không đúng định dạng UUID"
// @Failure      401     {object} map[string]string     "Chưa đăng nhập"
// @Failure      403     {object} map[string]string     "Không có quyền thao tác (Forbidden)"
// @Failure      404     {object} map[string]string     "Không tìm thấy ví của người dùng này"
// @Router       /wallets/users/{userId} [get]
func (h *WalletHandler) GetWalletByUserId(c *gin.Context) error {
	targetUserIdStr := c.Param("userId")
	targetUserId, err := uuid.Parse(targetUserIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.walletGW.GetByUserId(ctx, targetUserId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateWalletStatus
// @Summary      Cập nhật trạng thái ví (Admin)
// @Description  Dành cho Quản trị viên khóa (tạm dừng) hoặc mở khóa ví của người dùng.
// @Tags         Wallets
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                           true "UUID của ví cần cập nhật"
// @Param        request body     wallet.UpdateWalletStatusRequest true "Thông tin trạng thái mới"
// @Success      200     {object} map[string]string                "Thông báo cập nhật thành công { message: '...' }"
// @Failure      400     {object} map[string]string                "Dữ liệu đầu vào hoặc UUID ví không hợp lệ"
// @Failure      401     {object} map[string]string                "Chưa đăng nhập"
// @Failure      403     {object} map[string]string                "Không có quyền thao tác (Forbidden)"
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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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
// @Summary      Xử lý Callback VNPay Return (Nạp tiền ví)
// @Description  Endpoint hứng callback từ VNPay sau khi khách thanh toán xong. Thực hiện xác thực chữ ký gRPC và bẻ lái (redirect) về Frontend kèm theo trạng thái giao dịch.
// @Tags         Wallets
// @Produce      html
// @Param        redirectTo       query string false "Đường dẫn Frontend muốn bẻ lái về (VD: /my-wallet)"
// @Param        vnp_Amount       query string false "Số tiền giao dịch (nhân 100)"
// @Param        vnp_ResponseCode query string false "Mã phản hồi từ VNPay (00 là thành công)"
// @Param        vnp_TxnRef       query string false "Mã tham chiếu giao dịch nội bộ"
// @Param        vnp_SecureHash   query string false "Mã băm bảo mật checksum"
// @Success      302 "Chuyển hướng (Redirect) về ứng dụng Frontend"
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
