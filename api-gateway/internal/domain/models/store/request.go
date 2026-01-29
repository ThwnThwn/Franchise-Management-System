package store

import "api-gateway/internal/domain/models/common"

type GetStoresRequest struct {
	common.PaginationQuery
	Keyword string `form:"keyword"` // Tìm theo tên
}

type FindNearbyStoresRequest struct {
	UserLat  float64 `form:"userLat" binding:"required"`
	UserLong float64 `form:"userLong" binding:"required"`
	RadiusKm float64 `form:"radiusKm,default=5"` // Mặc định 5km nếu không truyền
	Limit    int32   `form:"limit,default=10"`   // Mặc định 10 quán
}
