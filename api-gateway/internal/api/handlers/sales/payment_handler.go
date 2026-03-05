package sales

import (
	"api-gateway/config"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"fmt"
	"net/http"
	"net/url"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type PaymentHandler struct {
	cfg       *config.Config
	paymentGW sales.IPaymentService
}

func NewPaymentHandler(
	cfg *config.Config,
	paymentGW sales.IPaymentService,
) *PaymentHandler {
	return &PaymentHandler{
		cfg:       cfg,
		paymentGW: paymentGW,
	}
}

// CreatePaymentUrl
// @Summary      Generate Payment URL
// @Description  Generate a payment link (VNPay) for the specific order.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                    true  "Order UUID"
// @Param        request  body     order.CreatePaymentRequest true "Payment details"
// @Success      200      {object} map[string]interface{}    "Payment URL"
// @Failure      400      {object} map[string]string         "Validation error"
// @Router       /orders/{id}/payment [post]
func (h *PaymentHandler) CreatePaymentUrl(c *gin.Context) error {
	var req order.CreatePaymentRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.paymentGW.CreatePaymentUrl(ctx, orderId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"paymentUrl": res,
	})
	return nil
}

// ProcessVnPayReturn
// @Summary      Handle VNPay Return URL
// @Description  Endpoint for VNPay to redirect the browser after payment. Verifies the signature via gRPC and redirects to the Frontend UI.
// @Tags         Payments
// @Accept       json
// @Produce      html
// @Param        vnp_Amount       query string false "Transaction Amount"
// @Param        vnp_BankCode     query string false "Bank Code"
// @Param        vnp_OrderInfo    query string false "Order Information"
// @Param        vnp_ResponseCode query string false "VNPAY Response Code"
// @Param        vnp_TxnRef       query string false "Transaction Reference"
// @Param        vnp_SecureHash   query string false "Security Hash"
// @Success      302 "Redirects to the Frontend React application"
// @Router       /orders/vnpay/return [get]
func (h *PaymentHandler) ProcessVnPayReturn(c *gin.Context) error {
	// 1. Phân loại Query Params: Lọc riêng vnp_ và nhặt redirectTo
	vnpayParams := make(map[string]string)
	redirectTo := "" // Biến chứa URL FE mong muốn

	for key, values := range c.Request.URL.Query() {
		if len(values) > 0 {
			if key == "redirectTo" {
				redirectTo = values[0]
			} else if strings.HasPrefix(key, "vnp_") {
				// CHỈ lấy param của VNPAY để gửi xuống .NET (Chống bể chữ ký)
				vnpayParams[key] = values[0]
			}
		}
	}

	// 2. Tầng bảo mật: Validate redirectTo (Chống Open Redirect) & Dọn rác từ FE
	// Nếu FE lỡ truyền full link (VD: http://localhost:3000/orders/123), ta gọt phần gốc đi
	if after, ok := strings.CutPrefix(redirectTo, h.cfg.FrontEndOrigin); ok {
		redirectTo = after
	}

	// LƯỚI LỌC BẢO MẬT NHIỀU LỚP:
	// 1. Phải bắt đầu bằng "/" (Đảm bảo là Relative Path)
	// 2. KHÔNG được bắt đầu bằng "//" (Trình duyệt sẽ bẻ lái sang domain khác, VD: //hacker.com)
	// 3. KHÔNG được chứa "http://" hay "https://" (Chặn tuyệt đối link ngoại lai như bro nói)
	if redirectTo == "" ||
		!strings.HasPrefix(redirectTo, "/") ||
		strings.HasPrefix(redirectTo, "//") ||
		strings.Contains(strings.ToLower(redirectTo), "http://") ||
		strings.Contains(strings.ToLower(redirectTo), "https://") {

		parsedUrl, err := url.Parse(h.cfg.VNPay.ClientReturnUrl)
		if err == nil && parsedUrl.Path != "" {
			redirectTo = parsedUrl.Path
		} else {
			redirectTo = "/orders"
		}
	}

	// 3. Gọi gRPC xuống .NET để verify chữ ký (Chỉ mang theo vnpayParams)
	ctx := c.Request.Context()
	isSuccess, msg, err := h.paymentGW.ProcessVnPayReturn(ctx, vnpayParams)

	// 4. Quyết định trạng thái thanh toán
	paymentStatus := "failed"
	if err == nil && isSuccess {
		paymentStatus = "success"
	}

	// 5. Build URL trả về cho Frontend
	// Lấy cái gốc (Ví dụ: http://localhost:3000) cộng với cái path FE muốn (Ví dụ: /orders/123)
	// Lưu ý: h.clientReturnUrl lúc này nên cấu hình trong .env là FRONTEND_URL (http://localhost:3000)

	// Dùng url.QueryEscape để url-encode thông báo lỗi tiếng Việt
	safeMsg := url.QueryEscape(msg)

	// Kết quả: http://localhost:3000/orders/123?payment_status=success&message=Success
	separator := "?"
	if strings.Contains(redirectTo, "?") {
		separator = "&"
	}

	finalRedirectUrl := fmt.Sprintf("%s%s%spayment_status=%s&message=%s",
		h.cfg.FrontEndOrigin,
		redirectTo,
		separator,
		paymentStatus,
		safeMsg,
	)

	// 6. Bẻ lái
	c.Redirect(http.StatusFound, finalRedirectUrl)
	return nil
}
