package usecases

import (
	"api-gateway/internal/application/abstractions"
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type ShiftUseCase struct {
	service franchiseGW.IShiftService
}

func NewShiftUseCasee(service franchiseGW.IShiftService) abstractions.IShiftUseCase {
	return &ShiftUseCase{service: service}
}

// GetUserCurrentShift implements abstractions.IShiftUseCase.
func (s *ShiftUseCase) GetUserCurrentShift(ctx context.Context, id uuid.UUID) (*shift.GetUserCurrentShiftResponse, error) {
	return s.service.GetUserCurrentShift(ctx, id)
}
