package franchise

import (
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type IShiftService interface {
	// Lấy lịch làm việc hôm nay của nhân viên (Để App biết đang làm ở Store nào)
	GetUserCurrentShift(ctx context.Context, id uuid.UUID) (
		*shift.GetUserCurrentShiftResponse, error)
}
