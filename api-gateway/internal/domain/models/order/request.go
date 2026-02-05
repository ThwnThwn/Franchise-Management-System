package order

import (
	"api-gateway/internal/application/constants/enum"
	"api-gateway/internal/domain/models/common"
)

type CreateOrderRequest struct {
	Items   []*CartItem `json:"items" binding:"required"`
	Notes   string      `json:"notes"`
	StoreId string      `json:"storeId" binding:"required"`
}

type CartItem struct {
	Sku      string `json:"sku" binding:"required"`
	Quantity *int32 `json:"quantity" binding:"required"`
}

type GetMyOrdersRequest struct {
	common.PaginationQuery
}

type CreatePaymentRequest struct {
	ReturnUrl     *string            `json:"returnUrl"`
	PaymentMethod enum.PaymentMethod `json:"paymentMethod" binding:"required"`
}
