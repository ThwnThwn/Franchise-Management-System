package mapper

import (
	"api-gateway/internal/domain/models/order"
	"api-gateway/internal/domain/models/store"
	"api-gateway/pkg/pb/sales"
	"api-gateway/pkg/utils/numberutils"
	"api-gateway/pkg/utils/timeutils"
)

// Interface định nghĩa những gì một "Order" cần có để được map
type IOrderWithStore interface {
	GetStoreId() string
	SetStore(s *store.StoreDetailResponse)
}

// Hàm Generic dùng chung cho []*Order, []*OrderSummary, ...
func MapStoreToOrderHeaders[T IOrderWithStore](entities []T, stores []*store.StoreDetailResponse) []T {
	if len(entities) == 0 {
		return entities
	}

	// 1. Tạo Map để lookup cho nhanh (O(1))
	storeMap := make(map[string]*store.StoreDetailResponse, len(stores))
	for _, s := range stores {
		if s != nil {
			storeMap[s.Id] = s
		}
	}

	// 2. Duyệt và gán Store
	for _, item := range entities {
		// item ở đây có thể là *Order hoặc *OrderSummary
		if s, ok := storeMap[item.GetStoreId()]; ok {
			item.SetStore(s)
		}
	}

	return entities
}

func MapCheckEnoughStockRes(s *sales.CheckEnoughStockResponse) *order.CheckEnoughStockResponse {
	items := make([]*order.ShortageItem, 0, len(s.ShortageItems))
	for _, item := range s.ShortageItems {
		items = append(items, &order.ShortageItem{
			VariantId:           item.VariantId,
			MaxFeasibleQuantity: item.MaxFeasibleQuantity,
		})
	}

	return &order.CheckEnoughStockResponse{
		IsEnough:      s.IsEnough,
		ShortageItems: items,
	}
}

func MapOrderSummaryPbToModel(orderPb *sales.OrderSummary) *order.OrderSummary {
	return &order.OrderSummary{
		OrderHeader: order.OrderHeader{
			Id:            orderPb.Id,
			Code:          orderPb.Code,
			Subtotal:      orderPb.Subtotal,
			Tax:           numberutils.GetFloat64(orderPb.Tax),
			TotalAmount:   orderPb.TotalAmount,
			PaidAmount:    numberutils.GetFloat64(orderPb.PaidAmount),
			PaymentMethod: numberutils.GetInt32(orderPb.PaymentMethod),
			Status:        numberutils.GetInt32(orderPb.Status),
			PaidAt:        timeutils.StringToTime(orderPb.PaidAt),
			Notes:         orderPb.Notes,
			CreatedAt:     timeutils.StringToTime(orderPb.CreatedAt),
			UpdatedAt:     timeutils.StringToTime(orderPb.UpdatedAt),

			CustomerId: orderPb.CustomerId,
			Store: &store.StoreDetailResponse{
				Id: orderPb.StoreId,
			},
		},
	}
}

func MapOrderPbToHeaderModel(orderPb *sales.Order) *order.OrderHeader {
	return &order.OrderHeader{
		Id:            orderPb.Id,
		Code:          orderPb.Code,
		Subtotal:      orderPb.Subtotal,
		Tax:           numberutils.GetFloat64(orderPb.Tax),
		TotalAmount:   orderPb.TotalAmount,
		PaidAmount:    orderPb.PaidAmount,
		PaymentMethod: numberutils.GetInt32(orderPb.PaymentMethod),
		Status:        numberutils.GetInt32(orderPb.Status),
		PaidAt:        timeutils.StringToTime(orderPb.PaidAt),
		Notes:         orderPb.Notes,
		CreatedAt:     timeutils.StringToTime(orderPb.CreatedAt),
		UpdatedAt:     timeutils.StringToTime(orderPb.UpdatedAt),

		CustomerId: orderPb.CustomerId,
		Store: &store.StoreDetailResponse{
			Id: orderPb.StoreId,
		},
	}
}
