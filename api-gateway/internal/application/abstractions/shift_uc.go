package abstractions

import (
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type IShiftUseCase interface {
	// Lấy lịch làm việc hôm nay của nhân viên (Để App biết đang làm ở Store nào)
	GetUserCurrentShift(ctx context.Context, id uuid.UUID) (
		*shift.GetUserCurrentShiftResponse, error)
}
