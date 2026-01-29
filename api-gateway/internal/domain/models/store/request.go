package store

import "api-gateway/internal/domain/models/common"

type GetStoresRequest struct {
	common.PaginationQuery
	Keyword string `form:"keyword"` // Tìm theo tên
}

type FindNearbyStoresRequest struct {
	UserLat  float64 `form:"userLat"`
	UserLong float64 `form:"userLong"`
	RadiusKm float64 `form:"radiusKm"` // Tìm trong bán kính 5km
	Limit    int32   `form:"limit"`
}
