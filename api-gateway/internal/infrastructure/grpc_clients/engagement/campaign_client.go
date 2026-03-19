package engagement

import (
	engagementGW "api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/campaign"
	"context"

	"api-gateway/pkg/mapper"
	engagementPb "api-gateway/pkg/pb/engagement"
	"api-gateway/pkg/utils/timeutils"

	"github.com/google/uuid"
	"google.golang.org/protobuf/types/known/emptypb"
)

type campaignGrpcClient struct {
	client engagementPb.CampaignGrpcClient
}

func ProvideCampaignClient(f *clientFactory) engagementGW.ICampaignService {
	return &campaignGrpcClient{client: engagementPb.NewCampaignGrpcClient(f.conn)}
}

// GetAll implements engagement.ICampaignService.
func (c *campaignGrpcClient) GetAll(ctx context.Context) ([]*campaign.CampaignResponse, error) {
	res, err := c.client.GetAll(ctx, &emptypb.Empty{})
	if err != nil {
		return []*campaign.CampaignResponse{}, err
	}

	items := make([]*campaign.CampaignResponse, 0, len(res.Campaigns))
	for _, item := range res.Campaigns {
		items = append(items, mapper.MapCampaignPbToModel(item))
	}

	return items, nil
}

// GetActive implements engagement.ICampaignService.
func (c *campaignGrpcClient) GetActive(ctx context.Context) ([]*campaign.CampaignResponse, error) {
	res, err := c.client.GetActive(ctx, &emptypb.Empty{})
	if err != nil {
		return []*campaign.CampaignResponse{}, err
	}

	items := make([]*campaign.CampaignResponse, 0, len(res.Campaigns))
	for _, item := range res.Campaigns {
		items = append(items, mapper.MapCampaignPbToModel(item))
	}

	return items, nil
}

// GetById implements engagement.ICampaignService.
func (c *campaignGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*campaign.CampaignResponse, error) {
	res, err := c.client.GetById(ctx, &engagementPb.GetByIdRequest{
		Id: id.String(),
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapCampaignPbToModel(res), nil
}

// Create implements engagement.ICampaignService.
func (c *campaignGrpcClient) Create(ctx context.Context, req *campaign.CreateCampaignRequest) error {
	_, err := c.client.CreateCampaign(ctx, &engagementPb.CreateCampaignRequest{
		Name:        req.Name,
		Description: req.Description,
		StartDate:   timeutils.ParseStringPtrToTimestamp(req.StartDate),
		EndDate:     timeutils.ParseStringPtrToTimestamp(req.EndDate),
		VoucherIds:  req.VoucherIds,
	})
	return err
}

// UpdateActive implements engagement.ICampaignService.
func (c *campaignGrpcClient) UpdateActive(ctx context.Context, id uuid.UUID, req *campaign.UpdateActiveStatusRequest) error {
	_, err := c.client.UpdateActiveStatus(ctx, &engagementPb.UpdateActiveStatusRequest{
		Id:       id.String(),
		IsActive: req.IsActive,
	})
	return err
}
