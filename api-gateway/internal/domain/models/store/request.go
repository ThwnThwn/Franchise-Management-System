package store

import "api-gateway/internal/domain/models/common"

type GetStoresRequest struct {
	common.PaginationQuery
	Keyword string `json:"keyword"` // Tìm theo tên
}

type FindNearbyStoresRequest struct {
	UserLat  float64 `json:"userLat"`
	UserLong float64 `json:"userLong"`
	RadiusKm float64 `json:"radiusKm"` // Tìm trong bán kính 5km
	Limit    int32   `json:"limit"`
}
