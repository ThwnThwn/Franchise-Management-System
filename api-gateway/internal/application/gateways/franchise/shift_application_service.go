package franchise

import (
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type IShiftApplicationService interface {
	Apply(ctx context.Context, shiftId, userId uuid.UUID, req *shift.ApplyForShiftRequest) error
	GetByShiftId(ctx context.Context, shiftId uuid.UUID) ([]*shift.ShiftApplicationItem, error)
	Review(ctx context.Context, appId, managerId uuid.UUID, req *shift.ReviewApplicationRequest) error
}
