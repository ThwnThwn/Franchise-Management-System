package mapper

import (
	"api-gateway/internal/domain/models/store"
	"api-gateway/pkg/pb/franchise"
)

func MapStorePbToStoreModel(s *franchise.StoreDetailResponse) *store.StoreDetailResponse {
	return &store.StoreDetailResponse{
		Id:        s.Id,
		Name:      s.Name,
		Address:   s.Address,
		Phone:     s.Phone,
		ImageUrl:  s.ImageUrl,
		Latitude:  s.Latitude,
		Longitude: s.Longitude,
		IsOpenNow: s.IsOpenNow,
	}
}
