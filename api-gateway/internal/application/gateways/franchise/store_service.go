package franchise

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/store"
	"context"

	"github.com/google/uuid"
)

type IStoreService interface {
	// Lấy danh sách cửa hàng (có phân trang & lọc)
	GetStores(ctx context.Context, req *store.GetStoresRequest) (
		common.PaginationResult[*store.StoreDetailResponse], error)

	// Lấy chi tiết 1 cửa hàng (để hiện địa chỉ, hình ảnh)
	GetStoreDetail(ctx context.Context, id uuid.UUID) (*store.StoreDetailResponse, error)

	// Tìm cửa hàng gần nhất (Dựa trên GPS User gửi lên)
	FindNearbyStores(ctx context.Context, req *store.FindNearbyStoresRequest) (
		common.PaginationResult[*store.StoreDetailResponse], error)

	// Kiểm tra cửa hàng có nhận đơn được không? (Mở cửa? Còn hoạt động?)
	CheckStoreAvailability(ctx context.Context, id uuid.UUID) (bool, string, error)
}
