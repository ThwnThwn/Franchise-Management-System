package supplier

import (
	"api-gateway/config"
	"api-gateway/internal/application/constants/errorcode"
	gw "api-gateway/internal/application/gateways/supplier"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
	"api-gateway/pkg/utils/validation"
	"fmt"
	"net/http"
	"net/url"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type SupplierPaymentHandler struct {
	cfg       *config.Config
	paymentGW gw.ISupplierPaymentService
}

func NewSupplierPaymentHandler(
	cfg *config.Config,
	paymentGW gw.ISupplierPaymentService,
) *SupplierPaymentHandler {
	return &SupplierPaymentHandler{
		cfg:       cfg,
		paymentGW: paymentGW,
	}
}

// Create
// @Summary      Tạo thanh toán cho đơn nhập hàng
// @Description  Khởi tạo thanh toán bằng Tiền mặt (CASH) hoặc lấy URL VNPay để thanh toán.
// @Tags         Suppliers / Payments
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body supplier.CreateSupplierPaymentReq true "Thông tin thanh toán"
// @Success      200 {object} supplier.CreateSupplierPaymentRes
// @Router       /suppliers/payments [post]
func (h *SupplierPaymentHandler) Create(c *gin.Context) error {
	var req supplier.CreateSupplierPaymentReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.paymentGW.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetAll
// @Summary      Toàn bộ lịch sử thanh toán nhà cung cấp
// @Description  Dành cho Admin/Kế toán theo dõi toàn bộ dòng tiền nhập hàng.
// @Tags         Suppliers / Payments
// @Produce      json
// @Security     BearerAuth
// @Param        query query supplier.GetAllSupplierPaymentsReq true "Phân trang và lọc"
// @Success      200 {object} common.PaginationResult[supplier.SupplierPaymentSummary]
// @Router       /suppliers/payments [get]
func (h *SupplierPaymentHandler) GetAll(c *gin.Context) error {
	var req supplier.GetAllSupplierPaymentsReq
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.paymentGW.GetAll(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetByPoId
// @Summary      Danh sách thanh toán của một PO
// @Description  Lấy lịch sử các đợt trả tiền của một đơn nhập hàng cụ thể.
// @Tags         Suppliers / Payments
// @Produce      json
// @Security     BearerAuth
// @Param        id path string true "UUID của đơn nhập hàng (Purchase Order)"
// @Success      200 {array} supplier.SupplierPaymentDetail
// @Router       /suppliers/purchase-orders/{id}/payments [get]
func (h *SupplierPaymentHandler) GetByPoId(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	res, err := h.paymentGW.GetByPoId(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// HandleVnPayReturn
// @Summary      Xử lý VNPay Return cho đơn nhập hàng (Supplier)
// @Description  Endpoint nhận redirect từ VNPay. Verify chữ ký qua gRPC và bẻ lái trình duyệt về giao diện Frontend.
// @Tags         Suppliers / Payments
// @Produce      html
// @Param        vnp_Amount       query string false "Số tiền giao dịch"
// @Param        vnp_BankCode     query string false "Mã ngân hàng"
// @Param        vnp_OrderInfo    query string false "Thông tin đơn hàng"
// @Param        vnp_ResponseCode query string false "Mã phản hồi VNPAY"
// @Param        vnp_TxnRef       query string false "Mã tham chiếu (Mã Payment)"
// @Param        vnp_SecureHash   query string false "Chữ ký bảo mật"
// @Param        redirectTo       query string false "Path của FE muốn quay về (VD: /suppliers/purchase-orders/123)"
// @Success      302 "Redirect về Frontend App"
// @Router       /suppliers/payments/vnpay/return [get]
func (h *SupplierPaymentHandler) HandleVnPayReturn(c *gin.Context) error {
	// 1. PHÂN LOẠI PARAMS: Lọc riêng vnp_ và nhặt redirectTo
	vnpayParams := make(map[string]string)
	redirectTo := ""

	for key, values := range c.Request.URL.Query() {
		if len(values) > 0 {
			if key == "redirectTo" {
				redirectTo = values[0]
			} else if strings.HasPrefix(key, "vnp_") {
				// CHỈ lấy param của VNPAY để gửi xuống .NET (Để .NET verify chữ ký chính xác)
				vnpayParams[key] = values[0]
			}
		}
	}

	// 2. BẢO MẬT: Validate redirectTo (Chặn Open Redirect)
	// Gọt phần Origin nếu FE truyền full link
	if after, ok := strings.CutPrefix(redirectTo, h.cfg.FrontEndOrigin); ok {
		redirectTo = after
	}

	// Kiểm tra tính hợp lệ của Path (Phải bắt đầu bằng / và không được là link ngoại lai)
	if redirectTo == "" ||
		!strings.HasPrefix(redirectTo, "/") ||
		strings.HasPrefix(redirectTo, "//") ||
		strings.Contains(strings.ToLower(redirectTo), "http://") ||
		strings.Contains(strings.ToLower(redirectTo), "https://") {

		// Fallback về trang danh sách thanh toán supplier mặc định trong config
		parsedUrl, err := url.Parse(h.cfg.VNPay.SupplierClientReturnUrl)
		if err == nil && parsedUrl.Path != "" {
			redirectTo = parsedUrl.Path
		} else {
			redirectTo = "/suppliers/payments"
		}
	}

	// 3. GỌI gRPC XUỐNG .NET ĐỂ VERIFY CHỮ KÝ
	ctx := c.Request.Context()
	res, err := h.paymentGW.HandleReturn(ctx, vnpayParams)

	// 4. QUYẾT ĐỊNH TRẠNG THÁI
	paymentStatus := "failed"
	msg := "Thanh toán thất bại hoặc lỗi chữ ký"

	if err == nil && res.Success {
		paymentStatus = "success"
		msg = "Thanh toán đơn nhập hàng thành công"
	} else if err != nil {
		msg = err.Error() // Lấy lỗi từ gRPC nếu có (VD: Invalid Signature)
	}

	// 5. BUILD URL TRẢ VỀ FRONTEND
	safeMsg := url.QueryEscape(msg)

	separator := "?"
	if strings.Contains(redirectTo, "?") {
		separator = "&"
	}

	// Kết quả cuối cùng: http://localhost:3000/path/to/fe?payment_status=success&message=...
	finalRedirectUrl := fmt.Sprintf("%s%s%spayment_status=%s&message=%s",
		h.cfg.FrontEndOrigin,
		redirectTo,
		separator,
		paymentStatus,
		safeMsg,
	)

	// 6. REDIRECT
	c.Redirect(http.StatusFound, finalRedirectUrl)
	return nil
}
