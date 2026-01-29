package usecases

import (
	"api-gateway/internal/application/abstractions"
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/store"
	"context"

	"github.com/google/uuid"
)

type StoreUseCase struct {
	service franchiseGW.IStoreService
}

func NewStoreUseCase(service franchiseGW.IStoreService) abstractions.IStoreUseCase {
	return &StoreUseCase{service: service}
}

// CheckStoreAvailability implements abstractions.IStoreUseCase.
func (s *StoreUseCase) CheckStoreAvailability(ctx context.Context, id uuid.UUID) (bool, string, error) {
	return s.service.CheckStoreAvailability(ctx, id)
}

// FindNearbyStores implements abstractions.IStoreUseCase.
func (s *StoreUseCase) FindNearbyStores(ctx context.Context, req *store.FindNearbyStoresRequest) (*common.PaginationResult[*store.StoreDetailResponse], error) {
	return s.service.FindNearbyStores(ctx, req)
}

// GetStoreDetail implements abstractions.IStoreUseCase.
func (s *StoreUseCase) GetStoreDetail(ctx context.Context, id uuid.UUID) (*store.StoreDetailResponse, error) {
	return s.service.GetStoreDetail(ctx, id)
}

// GetStores implements abstractions.IStoreUseCase.
func (s *StoreUseCase) GetStores(ctx context.Context, req *store.GetStoresRequest) (*common.PaginationResult[*store.StoreDetailResponse], error) {
	return s.service.GetStores(ctx, req)
}
