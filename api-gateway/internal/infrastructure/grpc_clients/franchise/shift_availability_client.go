package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/shift"
	"context"

	franchisePb "api-gateway/pkg/pb/franchise"
	"api-gateway/pkg/utils/boolutils"
	"api-gateway/pkg/utils/numberutils"
	"api-gateway/pkg/utils/stringutils"

	"github.com/google/uuid"
)

type shiftAvailabilityGrpcClient struct {
	client franchisePb.ShiftAvailabilityGrpcServiceClient
}

func ProvideShiftAvailabilityClient(f *clientFactory) franchiseGW.IShiftAvailabilityService {
	return &shiftAvailabilityGrpcClient{client: franchisePb.NewShiftAvailabilityGrpcServiceClient(f.conn)}
}

// GetAll implements franchise.IShiftAvailabilityService.
func (s *shiftAvailabilityGrpcClient) GetAll(ctx context.Context, req *shift.GetAvailabilityParams) (common.PaginationResult[*shift.ShiftAvailabilityResponse], error) {
	res, err := s.client.GetStaffAvailabilities(ctx, &franchisePb.GetAvailabilityParams{
		FromDate: stringutils.GetString(req.FromDate),
		ToDate:   stringutils.GetString(req.ToDate),

		HasIsAvailable: req.IsAvailable != nil,
		IsAvailable:    boolutils.GetBool(req.IsAvailable),

		HasIsRecurring: req.IsRecurring != nil,
		IsRecurring:    boolutils.GetBool(req.IsRecurring),

		HasDayOfWeek: req.DayOfWeek != nil,
		DayOfWeek:    numberutils.GetInt32(req.DayOfWeek),

		StoreId: req.StoreId,
		UserId:  req.UserId,
		Keyword: req.Keyword,

		Page:     req.Page,
		PageSize: req.PageSize,
	})
	if err != nil {
		return common.PaginationResult[*shift.ShiftAvailabilityResponse]{}, err
	}

	items := make([]*shift.ShiftAvailabilityResponse, 0, len(res.Data))
	for _, item := range res.Data {

		var dow *int32 = nil
		if item.DayOfWeek != -1 {
			dow = &item.DayOfWeek
		}

		items = append(items, &shift.ShiftAvailabilityResponse{
			Id:     item.Id,
			UserId: item.UserId,

			IsAvailable: item.IsAvailable,
			IsRecurring: item.IsRecurring,

			SpecificDate: stringutils.ToPtr(item.SpecificDate),
			DayOfWeek:    dow,

			StartTime: item.StartTime,
			EndTime:   item.EndTime,
			Note:      stringutils.ToPtr(item.Note),

			CreatedAt: item.CreatedAt,
			UpdatedAt: item.UpdatedAt,
		})
	}

	return common.PaginationResult[*shift.ShiftAvailabilityResponse]{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// Create implements franchise.IShiftAvailabilityService.
func (s *shiftAvailabilityGrpcClient) Create(ctx context.Context, userId uuid.UUID, req *shift.CreateShiftAvailabilityRequest) error {
	var dow int32 = -1
	if req.DayOfWeek != nil {
		dow = *req.DayOfWeek
	}
	_, err := s.client.CreateAvailability(ctx, &franchisePb.CreateStaffAvailabilityRequest{
		IsAvailable:  req.IsAvailable,
		IsRecurring:  req.IsRecurring,
		SpecificDate: stringutils.GetString(req.SpecificDate),
		DayOfWeek:    dow,
		StartTime:    req.StartTime,
		EndTime:      req.EndTime,
		Note:         stringutils.GetString(req.Note),
		UserId:       userId.String(),
	})
	return err
}

// Update implements franchise.IShiftAvailabilityService.
func (s *shiftAvailabilityGrpcClient) Update(ctx context.Context, id uuid.UUID, userId uuid.UUID, req *shift.UpdateShiftAvailabilityRequest) error {
	_, err := s.client.UpdateAvailability(ctx, &franchisePb.UpdateStaffAvailabilityRequest{
		Id: id.String(),

		HasIsAvailable: req.IsAvailable != nil,
		IsAvailable:    boolutils.GetBool(req.IsAvailable),

		HasIsRecurring: req.IsRecurring != nil,
		IsRecurring:    boolutils.GetBool(req.IsRecurring),

		SpecificDate: stringutils.GetString(req.SpecificDate),

		HasDayOfWeek: req.DayOfWeek != nil,
		DayOfWeek:    numberutils.GetInt32(req.DayOfWeek),

		StartTime: stringutils.GetString(req.StartTime),
		EndTime:   stringutils.GetString(req.EndTime),

		Note:   stringutils.GetString(req.Note),
		UserId: userId.String(),
	})
	return err
}

// Delete implements franchise.IShiftAvailabilityService.
func (s *shiftAvailabilityGrpcClient) Delete(ctx context.Context, id uuid.UUID, userId uuid.UUID) error {
	_, err := s.client.DeleteAvailability(ctx, &franchisePb.DeleteAvailabilityRequest{
		Id:     id.String(),
		UserId: userId.String(),
	})
	return err
}
