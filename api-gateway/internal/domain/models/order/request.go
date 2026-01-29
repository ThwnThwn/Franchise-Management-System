package order

import "api-gateway/internal/domain/models/common"

type CreateOrderRequest struct {
	Items         []*CartItem `json:"items" binding:"required"`
	Tax           *float64    `json:"tax"`
	PaymentMethod *int32      `json:"paymentMethod"`
	Notes         string      `json:"notes"`
	StoreId       string      `json:"storeId" binding:"required"`
}

type CartItem struct {
	Sku      string `json:"sku" binding:"required"`
	Quantity *int32 `json:"quantity" binding:"required"`
}

type GetMyOrdersRequest struct {
	common.PaginationQuery
}
