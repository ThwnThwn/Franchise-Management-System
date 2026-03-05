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
	Sku                 string `json:"sku"`
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

type GetRevenueReportReply struct {
	FromDate            string         `json:"fromDate" example:"2024-01-01T00:00:00Z"`
	ToDate              string         `json:"toDate" example:"2024-01-31T23:59:59Z"`
	CategoryId          string         `json:"categoryId"`
	Items               []*RevenueItem `json:"items"`
	OverallTotalRevenue float64        `json:"overallTotalRevenue"`
}

type GetOrderVolumeReply struct {
	Orders []*OrderVolumeADay `json:"orders"`
}

type RevenueItem struct {
	StoreId      *string `json:"storeId"`
	ProductName  string  `json:"productName"`
	Sku          string  `json:"sku"`
	QuantitySold int32   `json:"quantitySold"`
	UnitPrice    float64 `json:"unitPrice"`
	TotalRevenue float64 `json:"totalRevenue"`
}

type CsvFile struct {
	CsvRaw      []byte `json:"csvRaw"`
	CsvFileName string `json:"csvFileName"`
}

type OrderVolumeADay struct {
	Date           string           `json:"date" example:"2024-05-20"`
	TotalOrders    int32            `json:"totalOrders"`
	CanceledOrder  int32            `json:"canceledOrder"`
	CompletedOrder int32            `json:"completedOrder"`
	StoreId        *string          `json:"storeId"`
	StatusCounts   map[string]int32 `json:"statusCounts"`
}

type GetSalesTrendReply struct {
	DataPoints      []*SalesTrendDataPoint `json:"dataPoints"`
	PeakPeriodLabel *string                `json:"peakPeriodLabel"`
	SlowPeriodLabel *string                `json:"slowPeriodLabel"`
}

type SalesTrendDataPoint struct {
	TimeLabel   string  `json:"timeLabel"`
	OrderVolume int32   `json:"orderVolume"`
	Revenue     float64 `json:"revenue"`
}
