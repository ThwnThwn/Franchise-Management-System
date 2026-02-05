package mapper

import (
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/pb/sales"
)

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
