package order

type CreateOrderReply struct {
	OrderId     string  `json:"orderId"`
	OrderCode   string  `json:"orderCode"`
	TotalAmount float64 `json:"totalAmount"`
}

type Order struct {
	Id            string
	Code          string
	Subtotal      float64
	Tax           float64
	TotalAmount   float64
	PaidAmount    float64
	PaymentMethod int32
	Status        int32
	PaidAt        string
	Notes         string
	CreatedAt     string
	UpdatedAt     string
	Items         []*OrderItem
}

type OrderItem struct {
	Id          string
	ProductId   string
	VariantId   string
	Quantity    int32
	UnitPrice   float64
	ProductName string
	Sku         string
	ImageUrl    string
}
