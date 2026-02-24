package order

import (
	"api-gateway/internal/domain/models/store"
	"time"
)

type OrderReply struct {
	OrderId           string                    `json:"orderId"`
	OrderCode         string                    `json:"orderCode"`
	TotalAmount       float64                   `json:"totalAmount"`
	CustomerId        string                    `json:"customerId"`
	StoreId           string                    `json:"storeId"`
	CheckStockRespone *CheckEnoughStockResponse `json:"checkStockResponse"`
}

type CheckEnoughStockResponse struct {
	IsEnough      bool            `json:"isEnough"`
	ShortageItems []*ShortageItem `json:"shortageItems"`
}

type ShortageItem struct {
	VariantId           string `json:"variantId"`
	MaxFeasibleQuantity int32  `json:"maxFeasibleQuantity"`
}

type OrderHeader struct {
	Id            string                     `json:"id"`
	Code          string                     `json:"code"`
	Subtotal      float64                    `json:"subtotal"`
	Tax           float64                    `json:"tax"`
	TotalAmount   float64                    `json:"totalAmount"`
	PaidAmount    float64                    `json:"paidAmount"`
	PaymentMethod int32                      `json:"paymentMethod"`
	Status        int32                      `json:"status"`
	PaidAt        time.Time                  `json:"paidAt"`
	Notes         string                     `json:"notes"`
	CreatedAt     time.Time                  `json:"createdAt"`
	UpdatedAt     time.Time                  `json:"updatedAt"`
	CustomerId    string                     `json:"customerId"`
	Store         *store.StoreDetailResponse `json:"store"`
}

func (h *OrderHeader) GetStoreId() string {
	if h.Store != nil {
		return h.Store.Id
	}
	return ""
}

func (h *OrderHeader) SetStore(s *store.StoreDetailResponse) {
	h.Store = s
}

type Order struct {
	OrderHeader
	Items []*OrderItem `json:"items"`
}

type OrderSummary struct {
	OrderHeader
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
