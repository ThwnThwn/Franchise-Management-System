package vnpay

import (
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/application/gateways/supplier"
	"net/http"
	"strings"

	"github.com/gin-gonic/gin"
)

type VnPayHandler struct {
	salesGW    sales.IPaymentService
	supplierGW supplier.ISupplierPaymentService
}

func NewVnPayHandler(s sales.IPaymentService, sup supplier.ISupplierPaymentService) *VnPayHandler {
	return &VnPayHandler{
		salesGW:    s,
		supplierGW: sup,
	}
}

// HandleIPN
// @Summary      Handle VNPay IPN Webhook
// @Description  Receive Instant Payment Notification (IPN) callbacks from VNPay. Acts as a dispatcher to classify the transaction type (e.g., Customer Orders vs. Supplier Purchase Orders) and securely route the payload to the appropriate underlying Microservice via gRPC.
// @Tags         Payments
// @Produce      json
// @Success      200  {object} map[string]string "Always returns HTTP 200 with VNPay standard RspCode and Message"
// @Router       /vnpay/ipn [get]
func (h *VnPayHandler) HandleIPN(c *gin.Context) {
	// 1. Thu thập params
	rawQuery := c.Request.URL.Query()
	paramsMap := make(map[string]string)
	for k, v := range rawQuery {
		if len(v) > 0 {
			paramsMap[k] = v[0]
		}
	}

	orderInfo := strings.ToLower(paramsMap["vnp_OrderInfo"])
	txnRef := paramsMap["vnp_TxnRef"]

	// Biến chứa kết quả trả về cho VNPay
	var rspCode, message string

	// 2. PHÂN LUỒNG LOGIC (Dispatcher)

	// Check logic y hệt bên .NET ní đưa
	isSupplier := strings.Contains(orderInfo, "don nhap hang") ||
		strings.Contains(orderInfo, "po-") ||
		strings.HasPrefix(txnRef, "SUP")

	if isSupplier {
		// Gọi gRPC sang Supplier Service
		res, err := h.supplierGW.HandleIpn(c.Request.Context(), paramsMap)
		if err != nil {
			rspCode = "99"
			message = "Supplier Service gRPC error"
		} else {
			rspCode = res.RspCode
			message = res.Message
		}
	} else {
		// Gọi gRPC sang Sales Service (Đơn hàng, ví...)
		resCode, resMsg, err := h.salesGW.ProcessVnPayIpn(c.Request.Context(), paramsMap)
		if err != nil {
			rspCode = "99"
			message = "Sales Service gRPC error"
		} else {
			rspCode = resCode
			message = resMsg
		}
	}

	// 3. TRẢ VỀ JSON CHO VNPAY (Luôn luôn 200 OK)
	c.JSON(http.StatusOK, gin.H{
		"RspCode": rspCode,
		"Message": message,
	})
}
