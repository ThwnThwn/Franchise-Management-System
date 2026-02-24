package franchise

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type IShiftAvailabilityService interface {
	GetAll(ctx context.Context, req *shift.GetAvailabilityParams) (
		common.PaginationResult[*shift.ShiftAvailabilityResponse], error,
	)

	Create(ctx context.Context, userId uuid.UUID, req *shift.CreateShiftAvailabilityRequest) error
	Update(ctx context.Context, id uuid.UUID, userId uuid.UUID, req *shift.UpdateShiftAvailabilityRequest) error
	Delete(ctx context.Context, id uuid.UUID, userId uuid.UUID) error
}
