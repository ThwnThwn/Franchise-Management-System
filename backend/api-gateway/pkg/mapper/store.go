package mapper

import (
	"api-gateway/internal/domain/models/store"
	"api-gateway/pkg/pb/franchise"
	"api-gateway/pkg/utils/stringutils"
)

func MapStorePbToStoreModel(s *franchise.StoreDetailResponse) *store.StoreDetailResponse {
	if s == nil {
		return nil
	}

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

func MapAgendaStorePbToModel(s *franchise.AgendaStoreInfo) *store.StoreDetailResponse {
	if s == nil {
		return nil
	}

	return &store.StoreDetailResponse{
		Id:        s.Id,
		Name:      s.Name,
		Address:   s.Address,
		Phone:     stringutils.ToPtr(s.Phone),
		ImageUrl:  stringutils.ToPtr(s.ImageUrl),
		Latitude:  s.Latitude,
		Longitude: s.Longitude,
		IsOpenNow: s.IsOpenNow,
	}
}
