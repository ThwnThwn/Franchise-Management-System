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

		// IDs (Optional)
		AssignmentId: res.AssignmentId,
		ShiftId:      res.ShiftId,
		CurrentLogId: res.CurrentLogId,

		// Cửa hàng
		StoreId:   res.StoreId,
		StoreName: res.StoreName,
		StoreLat:  res.StoreLat,  // Map con trỏ *float64
		StoreLong: res.StoreLong, // Map con trỏ *float64

		// Thời gian ca làm
		StartTime: res.StartTime,
		EndTime:   res.EndTime,

		// Logic Flags
		CanCheckIn:  res.CanCheckIn,
		CanCheckOut: res.CanCheckOut,
		IsOvertime:  res.IsOvertime,

		// Timestamps (Optional)
		CheckInAt:  res.CheckInAt,
		CheckOutAt: res.CheckOutAt,
		ServerTime: res.ServerTime,
	}, nil
}
