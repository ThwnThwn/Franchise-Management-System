package franchise

import (
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type IShiftSwapService interface {
	RequestSwap(ctx context.Context, assignmentId uuid.UUID, sourceUserId uuid.UUID, req *shift.RequestSwapRequest) error
	RespondSwap(ctx context.Context, swapId uuid.UUID, userId uuid.UUID, req *shift.RespondSwapRequest) error
	ApproveSwap(ctx context.Context, swapId uuid.UUID, managerId uuid.UUID, req *shift.ApproveSwapRequest) error
	CancelSwap(ctx context.Context, swapId uuid.UUID, userId uuid.UUID) error
	Release(ctx context.Context, assignmentId uuid.UUID, userId uuid.UUID) error

	GetAll(ctx context.Context, req *shift.GetAllSwapsParams) ([]*shift.SwapRequestItem, error)
	GetMySwaps(ctx context.Context, userId uuid.UUID) ([]*shift.SwapRequestItem, error)
}
