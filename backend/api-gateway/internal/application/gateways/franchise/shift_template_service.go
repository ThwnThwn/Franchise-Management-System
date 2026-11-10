package franchise

import (
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type IShiftTemplateService interface {
	GetAll(ctx context.Context, req *shift.GetShiftTemplatesParams) (
		[]*shift.ShiftTemplateResponse, error,
	)
	GetById(ctx context.Context, id uuid.UUID) (*shift.ShiftTemplateResponse, error)

	Create(ctx context.Context, req *shift.CreateShiftTemplateRequest) error
	Update(ctx context.Context, id uuid.UUID, req *shift.UpdateShiftTemplateRequest) error
	Delete(ctx context.Context, id uuid.UUID) error
}
