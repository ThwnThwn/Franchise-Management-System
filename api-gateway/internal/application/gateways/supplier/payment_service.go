package supplier

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
	"context"

	"github.com/google/uuid"
)

type ISupplierPaymentService interface {
	// Tạo mới thanh toán cho PO (Cash hoặc lấy link VNPay)
	Create(ctx context.Context, req *supplier.CreateSupplierPaymentReq) (*supplier.CreateSupplierPaymentRes, error)

	// VNPay Return: Xử lý khi khách hàng bị redirect ngược về sau khi thanh toán
	HandleReturn(ctx context.Context, queryParams map[string]string) (*supplier.VnPayReturnRes, error)

	// VNPay IPN: Server-to-Server gọi từ VNPay để chốt trạng thái
	HandleIpn(ctx context.Context, queryParams map[string]string) (*supplier.VnPayIpnRes, error)

	// Lấy danh sách thanh toán của một đơn nhập hàng cụ thể
	GetByPoId(ctx context.Context, poId uuid.UUID) ([]*supplier.SupplierPaymentDetail, error)

	// Lấy toàn bộ lịch sử thanh toán supplier (phân trang, lọc)
	GetAll(ctx context.Context, req *supplier.GetAllSupplierPaymentsReq) (common.PaginationResult[*supplier.SupplierPaymentSummary], error)
}
