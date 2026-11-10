package order

import "api-gateway/internal/domain/constants/enum"

// Tương ứng với PaymentDto
type PaymentSummary struct {
	Id        string             `json:"id"`
	OrderId   string             `json:"orderId"`
	Code      string             `json:"code"`
	Amount    float64            `json:"amount"`
	Currency  string             `json:"currency"`
	Method    enum.PaymentMethod `json:"method"`
	Status    int32              `json:"status"`
	PaidAt    string             `json:"paidAt"`
	CreatedAt string             `json:"createdAt"`
}

// Tương ứng với PaymentHistoryItemDto cho Customer Dashboard
type PaymentHistoryItem struct {
	Id            string             `json:"id"`
	OrderId       string             `json:"orderId"`
	OrderCode     string             `json:"orderCode"`
	Amount        float64            `json:"amount"`
	Currency      string             `json:"currency"`
	PaymentMethod enum.PaymentMethod `json:"paymentMethod"`
	Status        int32              `json:"status"` // 0=Pending, 1=Paid, 2=Failed
	PaymentDate   string             `json:"paymentDate"`
}
