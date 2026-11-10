package mapper

import (
	"api-gateway/internal/domain/models/campaign"
	"api-gateway/internal/domain/models/voucher"
	pb "api-gateway/pkg/pb/engagement"
	"api-gateway/pkg/utils/timeutils"
)

func MapCampaignPbToModel(item *pb.CampaignResponse) *campaign.CampaignResponse {
	if item == nil {
		return nil
	}

	vouchers := make([]*voucher.VoucherResponse, 0, len(item.Vouchers))
	for _, v := range item.GetVouchers() {
		vouchers = append(vouchers, MapVoucherResponsePbToModel(v))
	}

	return &campaign.CampaignResponse{
		Id:          item.Id,
		Name:        item.Name,
		Description: item.Description,
		StartDate:   timeutils.ParseTimestampToStringPtr(item.StartDate),
		EndDate:     timeutils.ParseTimestampToStringPtr(item.EndDate),
		CreatedAt:   timeutils.ParseTimestampToStringPtr(item.CreatedAt),
		UpdatedAt:   timeutils.ParseTimestampToStringPtr(item.UpdatedAt),
		Vouchers:    vouchers,
		IsActive:    item.IsActive,
	}
}
