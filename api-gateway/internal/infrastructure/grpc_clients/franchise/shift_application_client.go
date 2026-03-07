package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	franchisePb "api-gateway/pkg/pb/franchise"
	"context"

	"github.com/google/uuid"
)

type shiftApplicationGrpcClient struct {
	client franchisePb.ShiftApplicationGrpcServiceClient
}

func ProvideShiftApplicationClient(f *clientFactory) franchiseGW.IShiftApplicationService {
	return &shiftApplicationGrpcClient{client: franchisePb.NewShiftApplicationGrpcServiceClient(f.conn)}
}

// Apply implements franchise.IShiftApplicationService.
func (s *shiftApplicationGrpcClient) Apply(ctx context.Context, shiftId uuid.UUID, userId uuid.UUID,
	req *shift.ApplyForShiftRequest,
) error {
	_, err := s.client.ApplyForShift(ctx, &franchisePb.ApplyForShiftRequest{
		ShiftId: shiftId.String(),
		UserId:  userId.String(),
		Note:    req.Note,
	})

	return err
}

// GetByShiftId implements franchise.IShiftApplicationService.
func (s *shiftApplicationGrpcClient) GetByShiftId(ctx context.Context, shiftId uuid.UUID) (
	[]*shift.ShiftApplicationItem, error,
) {
	res, err := s.client.GetApplicationsByShift(ctx, &franchisePb.GetApplicationsByShiftRequest{
		ShiftId: shiftId.String(),
	})
	if err != nil {
		return []*shift.ShiftApplicationItem{}, err
	}

	items := make([]*shift.ShiftApplicationItem, 0, len(res.Applications))
	for _, item := range res.Applications {
		items = append(items, &shift.ShiftApplicationItem{
			Id:                item.Id,
			ShiftId:           item.ShiftId,
			UserId:            item.UserId,
			Status:            item.Status,
			Note:              item.Note,
			AppliedAt:         item.AppliedAt,
			Username:          item.UserName,
			Email:             item.UserEmail,
			AvatarUrl:         item.AvatarUrl,
			HourlyWage:        item.HourlyWage,
			SmartScore:        item.SmartScore,
			WeeklyHoursWorked: item.WeeklyHoursWorked,
		})
	}

	return items, nil
}

// Review implements franchise.IShiftApplicationService.
func (s *shiftApplicationGrpcClient) Review(ctx context.Context,
	appId uuid.UUID, managerId uuid.UUID, req *shift.ReviewApplicationRequest,
) error {
	_, err := s.client.ReviewApplication(ctx, &franchisePb.ReviewApplicationRequest{
		ApplicationId: appId.String(),
		IsApproved:    req.IsApproved,
		ReviewedBy:    managerId.String(),
		ReviewNote:    req.ReviewNote,
	})

	return err
}
