package cart

type CartItemReply struct {
	Id         string  `json:"id"`
	Sku        string  `json:"sku"`
	CustomerId string  `json:"customerId"`
	Quantity   int32   `json:"quantity"`
	BasePrice  float64 `json:"basePrice"`
}
