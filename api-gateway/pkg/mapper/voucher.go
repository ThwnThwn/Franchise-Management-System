package mapper

import (
	"api-gateway/internal/domain/models/voucher"
	pb "api-gateway/pkg/pb/engagement"
	"api-gateway/pkg/utils/numberutils"
	"api-gateway/pkg/utils/timeutils"
)

func MapVoucherResponsePbToModel(v *pb.VoucherResponse) *voucher.VoucherResponse {
	if v == nil {
		return nil
	}

	voucherItems := make([]*voucher.VoucherItemResponse, 0, len(v.VoucherItems))
	for _, item := range v.VoucherItems {
		voucherItems = append(voucherItems, &voucher.VoucherItemResponse{
			Id:        item.Id,
			VariantId: item.VariantId,
			Quantity:  item.Quantity,
			Type:      numberutils.EnumPtrToInt32Ptr(item.Type),
		})
	}

	return &voucher.VoucherResponse{
		Id:               v.Id,
		Code:             v.Code,
		Type:             int32(v.Type),
		DiscountValue:    v.DiscountValue,
		MaxDiscountValue: v.MaxDiscountValue,
		MinOrderValue:    v.MinOrderValue,
		PointCost:        v.PointCost,
		InitQuantity:     v.InitQuantity,
		CurrentQuantity:  v.CurrentQuantity,
		MaxUsagePerUser:  v.MaxUsagePerUser,
		IsActive:         v.IsActive,
		EndDate:          timeutils.ParseTimestampToStringPtr(v.EndDate),
		CreatedAt:        timeutils.ParseTimestampToStringPtr(v.CreatedAt),
		UpdatedAt:        timeutils.ParseTimestampToStringPtr(v.UpdatedAt),
		VoucherItems:     voucherItems,
	}
}
