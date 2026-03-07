package order

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
)

type CreateOrderRequest struct {
	Items   []*CartItem `json:"items" binding:"required"`
	Notes   string      `json:"notes"`
	StoreId string      `json:"storeId" binding:"required"`
}

type CartItem struct {
	Sku      string `json:"sku" binding:"required"`
	Quantity int32  `json:"quantity" binding:"required,gt=0,max=100000"`
}

type GetMyOrdersRequest struct {
	common.PaginationQuery
}

type CreatePaymentRequest struct {
	ReturnUrl     *string            `json:"returnUrl"`
	PaymentMethod enum.PaymentMethod `json:"paymentMethod"`
}

type UpdateOrderStatus struct {
	Status enum.OrderStatus `json:"status" binding:"required"`
}

type UpdateOrderItem struct {
	Sku      string `json:"sku" binding:"required"`
	Quantity int32  `json:"quantity" binding:"required,gt=0,max=100000"`
}

type GetOrdersRequest struct {
	common.PaginationQuery
	StoreId       *string `form:"storeId"`
	Code          *string `form:"code"`
	Status        *int32  `form:"status"`
	PaymentMethod *int32  `form:"paymentMethod"`
	FromDate      *string `form:"fromDate"`
	ToDate        *string `form:"toDate"`
}

type GetRevenueReportRequest struct {
	FromDate   *string `form:"fromDate" example:"2024-01-01T00:00:00Z"`
	ToDate     *string `form:"toDate" example:"2024-01-31T23:59:59Z"`
	CategoryId *string `form:"categoryId"`
	StoreId    *string `form:"storeId"`
}

type GetOrderVolumeRequest struct {
	FromDate string            `form:"fromDate" example:"2024-05-20"`
	ToDate   string            `form:"toDate" example:"2024-05-27"`
	StoreId  *string           `form:"storeId"`
	Status   *enum.OrderStatus `form:"status"`
}

type GetSalesTrendRequest struct {
	// Thời gian bắt đầu. Bắt buộc nhập.
	FromDate string `form:"fromDate" binding:"required" example:"2024-03-01T00:00:00Z"`
	// Thời gian kết thúc. Bắt buộc nhập.
	ToDate string `form:"toDate" binding:"required" example:"2024-03-05T23:59:59Z"`
	// Mức độ gom nhóm dữ liệu. 0 = Hour, 1 = Day, 2 = Week.
	Aggregation int32 `form:"aggregation" enums:"0,1,2" default:"1" example:"1"`
	// ID Cửa hàng (Tùy chọn)
	StoreId *string `form:"storeId" example:"123e4567-e89b-12d3-a456-426614174000"`
	// ID Danh mục sản phẩm (Tùy chọn)
	CategoryId *string `form:"categoryId" example:"cat-001"`
}
