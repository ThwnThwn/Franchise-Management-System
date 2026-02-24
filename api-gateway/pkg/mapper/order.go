package mapper

import (
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/pb/sales"
	"api-gateway/pkg/utils/numberutils"
	"api-gateway/pkg/utils/timeutils"
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
	}
}
