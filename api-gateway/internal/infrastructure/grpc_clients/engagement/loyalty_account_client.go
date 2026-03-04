package engagement

import (
	engagementGW "api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/loyalty"
	"context"

	engagementPb "api-gateway/pkg/pb/engagement"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/timeutils"
)

type loyaltyAccountGrpcClient struct {
	client engagementPb.LoyaltyAccountGrpcClient
}

func ProvideLoyaltyAccountClient(f *clientFactory) engagementGW.ILoyaltyAccountService {
	return &loyaltyAccountGrpcClient{client: engagementPb.NewLoyaltyAccountGrpcClient(f.conn)}
}

// GetByUserPhoneOrEmail implements engagement.ILoyaltyAccountService.
func (l *loyaltyAccountGrpcClient) GetByUserPhoneOrEmail(ctx context.Context, req *loyalty.GetByUserPhoneOrEmailRequest) (
	[]*loyalty.LoyaltyAccountResponse, error,
) {
	res, err := l.client.GetByUserPhoneOrEmail(ctx, &engagementPb.GetByUserPhoneOrEmailRequest{
		PhoneNumber: stringutils.GetString(req.PhoneNumber),
		Email:       stringutils.GetString(req.Email),
	})
	if err != nil {
		return []*loyalty.LoyaltyAccountResponse{}, err
	}

	items := make([]*loyalty.LoyaltyAccountResponse, 0, len(res.AccountsRes))
	for _, item := range res.AccountsRes {
		var mappedTier *loyalty.MembershipTier
		if item.CurrentTier != nil {
			mappedTier = &loyalty.MembershipTier{
				Id:               item.CurrentTier.Id,
				Name:             item.CurrentTier.Name,
				MinSpendRequired: item.CurrentTier.MinSpendRequired,
				DiscountRate:     item.CurrentTier.DiscountRate,
				AmountPerPoint:   item.CurrentTier.AmountPerPoint,
				ValuePerPoint:    item.CurrentTier.ValuePerPoint,
			}
		}

		items = append(items, &loyalty.LoyaltyAccountResponse{
			LoyaltyResponse: loyalty.LoyaltyResponse{
				Id:              item.Id,
				AvailablePoints: item.AvailablePoints,
				TotalSpent:      item.TotalSpent,
				CurrentTier:     mappedTier,
			},
			UserName: item.UserName,
		})
	}

	return items, nil
}

// GetLoyaltyReport implements engagement.ILoyaltyAccountService.
func (l *loyaltyAccountGrpcClient) GetLoyaltyReport(ctx context.Context, req *loyalty.GetLoyaltyReportRequest) (
	*loyalty.LoyaltyReportResponse, error,
) {
	res, err := l.client.GetLoyaltyReport(ctx, &engagementPb.GetLoyaltyReportRequest{
		FromDate: timeutils.ParseStringPtrToTimestamp(req.FromDate),
		ToDate:   timeutils.ParseStringPtrToTimestamp(req.ToDate),
		TierId:   req.TierId,
		Format:   engagementPb.ExportFormat(req.Format),
	})
	if err != nil {
		return nil, err
	}

	tiers := make([]*loyalty.TierDistribution, 0, len(res.TierDistribution))
	for _, item := range res.TierDistribution {
		tiers = append(tiers, &loyalty.TierDistribution{
			TierName:    item.TierName,
			MemberCount: item.MemberCount,
		})
	}

	return &loyalty.LoyaltyReportResponse{
		TotalMembers:        res.TotalMembers,
		TotalPointsEarned:   res.TotalPointsEarned,
		TotalPointsRedeemed: res.TotalPointsRedeemed,
		FileContent:         res.FileContent,
		FileName:            res.FileName,
		TierDistribution:    tiers,
	}, nil
}
