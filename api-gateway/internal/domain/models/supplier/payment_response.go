package supplier

type SupplierPaymentSummary struct {
	Id                string  `json:"id"`
	PurchaseOrderId   string  `json:"purchaseOrderId"`
	PurchaseOrderCode string  `json:"purchaseOrderCode"`
	Code              string  `json:"code"`
	Amount            float64 `json:"amount"`
	Method            int32   `json:"method"`
	Status            int32   `json:"status"`
	PaidAt            string  `json:"paidAt"`
	CreatedAt         string  `json:"createdAt"`
}

type SupplierPaymentDetail struct {
	Id              string  `json:"id"`
	PurchaseOrderId string  `json:"purchaseOrderId"`
	Code            string  `json:"code"`
	Amount          float64 `json:"amount"`
	Method          int32   `json:"method"`
	Status          int32   `json:"status"`
	Notes           string  `json:"notes"`
	PaidAt          string  `json:"paidAt"`
	CreatedAt       string  `json:"createdAt"`
}

type CreateSupplierPaymentRes struct {
	PaymentId string `json:"paymentId"`
	Code      string `json:"code"`
	Url       string `json:"url"`
}

type VnPayReturnRes struct {
	Success         bool   `json:"success"`
	PaymentId       string `json:"paymentId"`
	PurchaseOrderId string `json:"purchaseOrderId"`
	ResponseCode    string `json:"responseCode"`
}

type VnPayIpnRes struct {
	RspCode string `json:"rspCode"`
	Message string `json:"message"`
}
