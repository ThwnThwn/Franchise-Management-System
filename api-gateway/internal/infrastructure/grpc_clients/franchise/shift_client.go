package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"context"

	franchisePb "api-gateway/pkg/pb/franchise"

	"github.com/google/uuid"
)

type shiftGrpcClient struct {
	client franchisePb.ShiftGrpcServiceClient
}

func ProvideShiftClient(f *clientFactory) franchiseGW.IShiftService {
	return &shiftGrpcClient{client: franchisePb.NewShiftGrpcServiceClient(f.conn)}
}

// GetUserCurrentShift implements franchise.IShiftService.
func (s *shiftGrpcClient) GetUserCurrentShift(ctx context.Context, id uuid.UUID) (*shift.GetUserCurrentShiftResponse, error) {
	res, err := s.client.GetUserCurrentShift(ctx, &franchisePb.GetUserCurrentShiftRequest{
		UserId: id.String(),
	})
	if err != nil {
		return nil, err
	}

	return &shift.GetUserCurrentShiftResponse{
		HasShiftToday: res.HasShiftToday,
		StoreId:       res.StoreId,
		StoreName:     res.StoreName,
		ShiftStart:    res.ShiftStart,
		ShiftEnd:      res.ShiftEnd,
		StoreLat:      res.StoreLat,
		StoreLong:     res.StoreLong,
	}, nil
}
