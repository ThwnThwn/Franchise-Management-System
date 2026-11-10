package cart

type CartItemReply struct {
	Id          string  `json:"id"`
	Sku         string  `json:"sku"`
	ProductName string  `json:"productName"`
	ImageUrl    string  `json:"imageUrl"`
	Attributes  string  `json:"attributes"`
	CustomerId  string  `json:"customerId"`
	Quantity    int32   `json:"quantity"`
	BasePrice   float64 `json:"basePrice"`
}
