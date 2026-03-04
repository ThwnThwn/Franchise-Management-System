package engagement

import (
	engagementGW "api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/membershiptier"
	"context"

	engagementPb "api-gateway/pkg/pb/engagement"

	"github.com/google/uuid"
	"google.golang.org/protobuf/types/known/emptypb"
)

type membershipTierGrpcClient struct {
	client engagementPb.MembershipTierGrpcClient
}

func ProvideMembershipTierClient(f *clientFactory) engagementGW.IMembershipTierService {
	return &membershipTierGrpcClient{client: engagementPb.NewMembershipTierGrpcClient(f.conn)}
}

// GetAll implements engagement.IMembershipTierService.
func (m *membershipTierGrpcClient) GetAll(ctx context.Context) ([]*membershiptier.MembershipTierResponse, error) {
	res, err := m.client.GetAllMembershipTiers(ctx, &emptypb.Empty{})
	if err != nil {
		return []*membershiptier.MembershipTierResponse{}, err
	}

	items := make([]*membershiptier.MembershipTierResponse, 0, len(res.Tiers))
	for _, item := range res.Tiers {
		items = append(items, &membershiptier.MembershipTierResponse{
			Id:               item.Id,
			Name:             item.Name,
			MinSpendRequired: item.MinSpendRequired,
			DiscountRate:     item.DiscountRate,
			AmountPerPoint:   item.AmountPerPoint,
			ValuePerPoint:    item.ValuePerPoint,
		})
	}

	return items, nil
}

// Create implements engagement.IMembershipTierService.
func (m *membershipTierGrpcClient) Create(ctx context.Context, req *membershiptier.CreateMembershipTierRequest) error {
	_, err := m.client.CreateMembershipTier(ctx, &engagementPb.CreateMembershipTierRequest{
		Name:             req.Name,
		MinSpendRequired: req.MinSpendRequired,
		DiscountRate:     req.DiscountRate,
		AmountPerPoint:   req.AmountPerPoint,
		ValuePerPoint:    req.ValuePerPoint,
	})
	return err
}

// Update implements engagement.IMembershipTierService.
func (m *membershipTierGrpcClient) Update(ctx context.Context, id uuid.UUID, req *membershiptier.UpdateMembershipTierRequest) error {
	_, err := m.client.UpdateMembershipTier(ctx, &engagementPb.UpdateMembershipTierRequest{
		Id:               id.String(),
		Name:             req.Name,
		MinSpendRequired: req.MinSpendRequired,
		DiscountRate:     req.DiscountRate,
		AmountPerPoint:   req.AmountPerPoint,
		ValuePerPoint:    req.ValuePerPoint,
	})
	return err
}

// Delete implements engagement.IMembershipTierService.
func (m *membershipTierGrpcClient) Delete(ctx context.Context, id uuid.UUID) error {
	_, err := m.client.DeleteMembershipTier(ctx, &engagementPb.DeleteMembershipTierRequest{
		Id: id.String(),
	})
	return err
}
