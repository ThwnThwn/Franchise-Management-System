package order

type CreateOrderRequest struct {
	Items         []*CartItem `json:"items"`
	Tax           *float64    `json:"tax"`
	PaymentMethod *int32      `json:"paymentMethod"`
	Notes         string      `json:"notes"`
}

type CartItem struct {
	Sku      string `json:"sku"`
	Quantity *int32 `json:"quantity"`
}
