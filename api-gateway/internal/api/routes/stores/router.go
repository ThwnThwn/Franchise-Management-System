package stores

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"

	"github.com/gin-gonic/gin"
)

type StoreRouter struct{}

func (s *StoreRouter) NewStoreRouter(cfg *config.Config,
	r *gin.RouterGroup,
	storeHandler *handlers.StoreHandler,
) {
	storeApi := r.Group("/stores")
	{
		// Lấy danh sách tất cả store
		storeApi.GET("", storeHandler.GetStores)
		// Lấy detail của 1 store
		storeApi.GET("/:id", storeHandler.GetStoreDetail)
		// Check xem store có đang mở cửa không
		storeApi.GET("/:id/availability", storeHandler.CheckStoreAvailability)
		// Nhận vào kinh độ, vĩ độ và tìm các cửa hàng gần nhất (theo bán kính xung quanh)
		storeApi.GET("/nearby", storeHandler.FindNearbyStores)
	}
}
