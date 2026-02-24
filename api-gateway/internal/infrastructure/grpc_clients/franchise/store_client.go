package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/store"
	"context"

	"api-gateway/pkg/mapper"
	franchisePb "api-gateway/pkg/pb/franchise"

	"github.com/google/uuid"
)

type storeGrpcClient struct {
	client franchisePb.StoreGrpcServiceClient
}

func ProvideStoreClient(f *clientFactory) franchiseGW.IStoreService {
	return &storeGrpcClient{client: franchisePb.NewStoreGrpcServiceClient(f.conn)}
}

// CheckStoreAvailability implements franchise.IStoreService.
func (s *storeGrpcClient) CheckStoreAvailability(ctx context.Context, id uuid.UUID) (bool, string, error) {
	res, err := s.client.CheckStoreAvailability(ctx, &franchisePb.CheckStoreAvailabilityRequest{
		StoreId: id.String(),
	})
	if err != nil {
		return false, "", err
	}
	return res.IsAvailable, res.Message, nil
}

// FindNearbyStores implements franchise.IStoreService.
func (s *storeGrpcClient) FindNearbyStores(
	ctx context.Context,
	req *store.FindNearbyStoresRequest,
) (common.PaginationResult[*store.StoreDetailResponse], error) {
	res, err := s.client.FindNearbyStores(ctx, &franchisePb.FindNearbyStoresRequest{
		UserLat:  req.UserLat,
		UserLong: req.UserLong,
		RadiusKm: req.RadiusKm,
		Limit:    req.Limit,
	})
	if err != nil {
		return common.PaginationResult[*store.StoreDetailResponse]{}, err
	}

	stores := make([]*store.StoreDetailResponse, 0, len(res.Items))
	for _, s := range res.Items {
		stores = append(stores, mapper.MapStorePbToStoreModel(s))
	}

	return common.PaginationResult[*store.StoreDetailResponse]{
		Items:      stores,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// GetStoreDetail implements franchise.IStoreService.
func (s *storeGrpcClient) GetStoreDetail(ctx context.Context, id uuid.UUID) (*store.StoreDetailResponse, error) {
	res, err := s.client.GetStoreDetail(ctx, &franchisePb.GetStoreDetailRequest{
		Id: id.String(),
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapStorePbToStoreModel(res), nil
}

// GetStores implements franchise.IStoreService.
func (s *storeGrpcClient) GetStores(ctx context.Context, req *store.GetStoresRequest) (common.PaginationResult[*store.StoreDetailResponse], error) {
	res, err := s.client.GetStores(ctx, &franchisePb.GetStoresRequest{
		Page:     req.Page,
		PageSize: req.PageSize,
		Keyword:  req.Keyword,
	})
	if err != nil {
		return common.PaginationResult[*store.StoreDetailResponse]{}, err
	}

	stores := make([]*store.StoreDetailResponse, 0, len(res.Items))
	for _, s := range res.Items {
		stores = append(stores, mapper.MapStorePbToStoreModel(s))
	}

	return common.PaginationResult[*store.StoreDetailResponse]{
		Items:      stores,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}
