package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/mapper"
	franchisePb "api-gateway/pkg/pb/franchise"
	"api-gateway/pkg/utils/stringutils"
	"context"

	"github.com/google/uuid"
)

type shiftRuleGrpcClient struct {
	client franchisePb.ShiftRuleGrpcServiceClient
}

func ProvideShiftRuleClient(f *clientFactory) franchiseGW.IShiftRuleService {
	return &shiftRuleGrpcClient{client: franchisePb.NewShiftRuleGrpcServiceClient(f.conn)}
}

// GetSettings implements franchise.IShiftRuleService.
func (s *shiftRuleGrpcClient) GetSettings(ctx context.Context, storeId string) (shift.ShiftRuleSettingsResponse, error) {
	res, err := s.client.GetShiftRuleSettings(ctx, &franchisePb.GetShiftRuleSettingsRequest{
		StoreId: storeId,
	})
	if err != nil {
		return shift.ShiftRuleSettingsResponse{}, err
	}

	settings := make([]*shift.ShiftRuleResponse, 0, len(res.Stores))
	for _, item := range res.Stores {
		settings = append(settings, mapper.MapShiftRulePbToModel(item))
	}

	return shift.ShiftRuleSettingsResponse{
		Global: mapper.MapShiftRulePbToModel(res.Global),
		Stores: settings,
	}, nil
}

// Save implements franchise.IShiftRuleService.
func (s *shiftRuleGrpcClient) Save(ctx context.Context, userId uuid.UUID, req *shift.SaveShiftRuleRequest) error {
	_, err := s.client.SaveShiftRule(ctx, &franchisePb.SaveShiftRuleRequest{
		StoreId:               stringutils.GetString(req.StoreID),
		AllowShuttle:          req.AllowShuttle,
		AllowShiftSwap:        req.AllowShiftSwap,
		MinHoursBetweenShifts: req.MinHoursBetweenShifts,
		MaxHoursPerWeek:       req.MaxHoursPerWeek,
		UserId:                userId.String(),
	})
	return err
}
