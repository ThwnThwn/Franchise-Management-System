package campaign

import (
	"api-gateway/internal/domain/models/voucher"
)

type CampaignResponse struct {
	Id          string                     `json:"id"`
	Name        string                     `json:"name"`
	Description string                     `json:"description"`
	StartDate   *string                    `json:"startDate"`
	EndDate     *string                    `json:"endDate"`
	CreatedAt   *string                    `json:"createdAt"`
	UpdatedAt   *string                    `json:"updatedAt"`
	Vouchers    []*voucher.VoucherResponse `json:"vouchers"`
	IsActive    bool                       `json:"isActive"`
}
