package engagement

import (
	"api-gateway/internal/domain/models/campaign"
	"context"

	"github.com/google/uuid"
)

type ICampaignService interface {
	GetAll(ctx context.Context) ([]*campaign.CampaignResponse, error)
	GetActive(ctx context.Context) ([]*campaign.CampaignResponse, error)
	GetById(ctx context.Context, id uuid.UUID) (*campaign.CampaignResponse, error)

	Create(ctx context.Context, req *campaign.CreateCampaignRequest) error
	UpdateActive(ctx context.Context, id uuid.UUID, req *campaign.UpdateActiveStatusRequest) error
	RedeemVoucher(ctx context.Context, userId, voucherId uuid.UUID) error
}
