package stores

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (s *StoreRouter) Init(r *gin.RouterGroup) {
	storeApi := r.Group("/stores")
	{
		// Lấy danh sách tất cả store
		storeApi.GET("", api.WrapHandler(s.storeHl.GetStores))

		// Nhận vào kinh độ, vĩ độ và tìm các cửa hàng gần nhất (theo bán kính xung quanh)
		storeApi.GET("/nearby", api.WrapHandler(s.storeHl.FindNearbyStores))

		// Lấy detail của 1 store
		storeApi.GET("/:id", api.WrapHandler(s.storeHl.GetStoreDetail))

		// Check xem store có đang mở cửa không
		storeApi.GET("/:id/availability", api.WrapHandler(s.storeHl.CheckStoreAvailability))
	}

	privateApi := r.Group("/stores")
	privateApi.Use(s.authMW.ValidateToken([]byte(s.cfg.JwtSecret)))
	{
		privateApi.GET("/:id/staffs/active", s.authMW.PermissionCheck("store.staff.read"),
			api.WrapHandler(s.storeHl.GetActiveStaffsAtStore))
	}

	s.stockR.Init(storeApi)
}
