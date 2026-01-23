package usecases

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/gateways"
	"context"
)

type CalcUseCase struct {
	service gateways.ICalcService
}

func NewCalcUseCase(service gateways.ICalcService) abstractions.ICalcUseCase {
	return &CalcUseCase{service: service}
}

func (uc *CalcUseCase) ExecuteAdd(ctx context.Context, a int32, b int32) (int32, error) {
	// Ở đây có thể thêm logic: Log audit, transform data, gọi thêm Auth...
	return uc.service.Add(ctx, a, b)
}
