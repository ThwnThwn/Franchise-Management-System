package stores

import (
	"github.com/gin-gonic/gin"
)

func (s *StoreRouter) Init(r *gin.RouterGroup) {
	storeApi := r.Group("/stores")
	{
		// Lấy danh sách tất cả store
		storeApi.GET("", s.storeHl.GetStores)
		// Nhận vào kinh độ, vĩ độ và tìm các cửa hàng gần nhất (theo bán kính xung quanh)
		storeApi.GET("/nearby", s.storeHl.FindNearbyStores)
		// Lấy detail của 1 store
		storeApi.GET("/:id", s.storeHl.GetStoreDetail)
		// Check xem store có đang mở cửa không
		storeApi.GET("/:id/availability", s.storeHl.CheckStoreAvailability)
	}

	s.stockR.Init(storeApi)
}
