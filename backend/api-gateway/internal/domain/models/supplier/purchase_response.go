package supplier

// --- PURCHASE ORDER RESPONSES ---

type PurchaseOrderItemDetail struct {
	Id           string  `json:"id"`
	IngredientId string  `json:"ingredientId"`
	Quantity     float64 `json:"quantity"`
	UnitPrice    float64 `json:"unitPrice"`
	TotalPrice   float64 `json:"totalPrice"`
}

type PurchaseOrderSummary struct {
	Id            string  `json:"id"`
	Code          string  `json:"code"`
	SupplierName  string  `json:"supplierName"`
	Status        int32   `json:"status"`
	PaymentStatus int32   `json:"paymentStatus"`
	TotalAmount   float64 `json:"totalAmount"`
	NetAmount     float64 `json:"netAmount"`
	OrderDate     string  `json:"orderDate"`
	CreatedAt     string  `json:"createdAt"`
}

type PurchaseOrderDetail struct {
	PurchaseOrderSummary
	SupplierId     string                    `json:"supplierId"`
	StoreId        string                    `json:"storeId"`
	DiscountAmount float64                   `json:"discountAmount"`
	PaidAmount     float64                   `json:"paidAmount"`
	CompletedAt    string                    `json:"completedAt"`
	Notes          string                    `json:"notes"`
	Items          []PurchaseOrderItemDetail `json:"items"`
	UpdatedAt      string                    `json:"updatedAt"`
}

type PurchaseOrderBaseRes struct {
	Id      string `json:"id,omitempty"`
	Code    string `json:"code,omitempty"`
	Message string `json:"message"`
}
