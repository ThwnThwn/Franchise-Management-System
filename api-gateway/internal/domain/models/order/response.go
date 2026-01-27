package order

import "time"

type CreateOrderReply struct {
	OrderId     string  `json:"orderId"`
	OrderCode   string  `json:"orderCode"`
	TotalAmount float64 `json:"totalAmount"`
}

type Order struct {
	Id            string       `json:"id"`
	Code          string       `json:"code"`
	Subtotal      float64      `json:"subtotal"`
	Tax           float64      `json:"tax"`
	TotalAmount   float64      `json:"totalAmount"`
	PaidAmount    float64      `json:"paidAmount"`
	PaymentMethod int32        `json:"paymentMethod"`
	Status        int32        `json:"status"`
	PaidAt        time.Time    `json:"paidAt"`
	Notes         string       `json:"notes"`
	CreatedAt     time.Time    `json:"createdAt"`
	UpdatedAt     time.Time    `json:"updatedAt"`
	Items         []*OrderItem `json:"items"`
}

type OrderItem struct {
	Id          string  `json:"id"`
	ProductId   string  `json:"productId"`
	VariantId   string  `json:"variantId"`
	Quantity    int32   `json:"quantity"`
	UnitPrice   float64 `json:"unitPrice"`
	ProductName string  `json:"productName"`
	Sku         string  `json:"sku"`
	ImageUrl    string  `json:"imageUrl"`
}
