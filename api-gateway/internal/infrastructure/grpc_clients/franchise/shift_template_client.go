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

type shiftTemplateGrpcClient struct {
	client franchisePb.ShiftTemplateGrpcServiceClient
}

func ProvideShiftTemplateClient(f *clientFactory) franchiseGW.IShiftTemplateService {
	return &shiftTemplateGrpcClient{client: franchisePb.NewShiftTemplateGrpcServiceClient(f.conn)}
}

// GetAll implements franchise.IShiftTemplateService.
func (s *shiftTemplateGrpcClient) GetAll(ctx context.Context, req *shift.GetShiftTemplatesParams) ([]*shift.ShiftTemplateResponse, error) {
	res, err := s.client.GetShiftTemplates(ctx, &franchisePb.GetShiftTemplatesRequest{
		StoreId:  stringutils.GetString(req.StoreID),
		Keyword:  stringutils.GetString(req.Keyword),
		IsActive: req.IsActive,
	})
	if err != nil {
		return []*shift.ShiftTemplateResponse{}, err
	}

	items := make([]*shift.ShiftTemplateResponse, 0, len(res.Items))
	for _, item := range res.Items {
		items = append(items, mapper.MapShiftTemplatePbToModel(item))
	}
	return items, nil
}

// GetById implements franchise.IShiftTemplateService.
func (s *shiftTemplateGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*shift.ShiftTemplateResponse, error) {
	res, err := s.client.GetShiftTemplateById(ctx, &franchisePb.GetShiftTemplateByIdRequest{
		Id: id.String(),
	})
	if err != nil {
		return nil, err
	}
	return mapper.MapShiftTemplatePbToModel(res), nil
}

// Create implements franchise.IShiftTemplateService.
func (s *shiftTemplateGrpcClient) Create(ctx context.Context, req *shift.CreateShiftTemplateRequest) error {
	_, err := s.client.CreateShiftTemplate(ctx, &franchisePb.CreateShiftTemplateRequest{
		StoreId:              stringutils.GetString(req.StoreID),
		Code:                 req.Code,
		Name:                 req.Name,
		StartTime:            req.StartTime,
		EndTime:              req.EndTime,
		BreakDurationMinutes: req.BreakDurationMinutes,
	})
	return err
}

// Update implements franchise.IShiftTemplateService.
func (s *shiftTemplateGrpcClient) Update(ctx context.Context, id uuid.UUID, req *shift.UpdateShiftTemplateRequest) error {
	_, err := s.client.UpdateShiftTemplate(ctx, &franchisePb.UpdateShiftTemplateRequest{
		Id:                   id.String(),
		Code:                 req.Code,
		Name:                 req.Name,
		StartTime:            req.StartTime,
		EndTime:              req.EndTime,
		BreakDurationMinutes: req.BreakDurationMinutes,
		IsActive:             req.IsActive,
	})
	return err
}

// Delete implements franchise.IShiftTemplateService.
func (s *shiftTemplateGrpcClient) Delete(ctx context.Context, id uuid.UUID) error {
	_, err := s.client.DeleteShiftTemplate(ctx, &franchisePb.DeleteShiftTemplateRequest{
		Id: id.String(),
	})
	return err
}
