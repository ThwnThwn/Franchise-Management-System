package supplier

import (
	"api-gateway/config"
	gw "api-gateway/internal/application/gateways/supplier"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
	pb "api-gateway/pkg/pb/supplier"
	"context"
	"math"

	"github.com/google/uuid"
)

type supplierPaymentGrpcClient struct {
	client pb.SupplierPaymentGrpcClient
	cfg    *config.Config
}

func NewSupplierPaymentClient(f *clientFactory, cfg *config.Config) gw.ISupplierPaymentService {
	return &supplierPaymentGrpcClient{
		client: pb.NewSupplierPaymentGrpcClient(f.conn),
		cfg:    cfg,
	}
}

// Create: Tạo thanh toán (Cash hoặc lấy URL VNPay)
func (s *supplierPaymentGrpcClient) Create(ctx context.Context, req *supplier.CreateSupplierPaymentReq) (*supplier.CreateSupplierPaymentRes, error) {
	var returnUrl string
	if req.ReturnUrl != nil && *req.ReturnUrl != "" {
		returnUrl = *req.ReturnUrl
	} else {
		// Fallback về config mặc định cho Supplier
		returnUrl = s.cfg.VNPay.SupplierClientReturnUrl
	}

	res, err := s.client.CreatePoPayment(ctx, &pb.CreatePoPaymentRequest{
		PurchaseOrderId: req.PurchaseOrderId,
		Method:          pb.SupplierPaymentMethodType(req.Method),
		ReturnUrl:       &returnUrl,
	})
	if err != nil {
		return nil, err
	}

	return &supplier.CreateSupplierPaymentRes{
		PaymentId: res.PaymentId,
		Code:      res.Code,
		Url:       res.Url,
	}, nil
}

// HandleReturn: Chuyển tiếp query params xuống .NET để verify chữ ký (Dùng cho browser redirect)
func (s *supplierPaymentGrpcClient) HandleReturn(ctx context.Context, queryParams map[string]string) (*supplier.VnPayReturnRes, error) {
	res, err := s.client.HandleVnPayReturn(ctx, &pb.HandleVnPayReturnRequest{
		QueryParams: queryParams,
	})
	if err != nil {
		return nil, err
	}

	return &supplier.VnPayReturnRes{
		Success:         res.Success,
		PaymentId:       res.PaymentId,
		PurchaseOrderId: res.PurchaseOrderId,
		ResponseCode:    res.ResponseCode,
	}, nil
}

// HandleIpn: Chuyển tiếp query params xuống .NET (Dùng cho server-to-server call)
func (s *supplierPaymentGrpcClient) HandleIpn(ctx context.Context, queryParams map[string]string) (*supplier.VnPayIpnRes, error) {
	res, err := s.client.HandleVnPayIpn(ctx, &pb.HandleVnPayIpnRequest{
		QueryParams: queryParams,
	})
	if err != nil {
		return nil, err
	}

	return &supplier.VnPayIpnRes{
		RspCode: res.RspCode,
		Message: res.Message,
	}, nil
}

// GetById (GetByPO): Lấy lịch sử thanh toán của một đơn nhập hàng
func (s *supplierPaymentGrpcClient) GetByPoId(ctx context.Context, poId uuid.UUID) ([]*supplier.SupplierPaymentDetail, error) {
	res, err := s.client.GetPaymentsByPurchaseOrder(ctx, &pb.GetPaymentsByPurchaseOrderRequest{
		PurchaseOrderId: poId.String(),
	})
	if err != nil {
		return nil, err
	}

	// Pre-allocation cho slice để tối ưu bộ nhớ
	payments := make([]*supplier.SupplierPaymentDetail, 0, len(res.Payments))
	for _, p := range res.Payments {
		payments = append(payments, &supplier.SupplierPaymentDetail{
			Id:              p.Id,
			PurchaseOrderId: p.PurchaseOrderId,
			Code:            p.Code,
			Amount:          p.Amount,
			Method:          p.Method,
			Status:          p.Status,
			Notes:           p.Notes,
			PaidAt:          p.PaidAt,
			CreatedAt:       p.CreatedAt,
		})
	}

	return payments, nil
}

// GetAll: Lấy toàn bộ lịch sử thanh toán supplier (có phân trang)
func (s *supplierPaymentGrpcClient) GetAll(ctx context.Context, req *supplier.GetAllSupplierPaymentsReq) (common.PaginationResult[*supplier.SupplierPaymentSummary], error) {
	res, err := s.client.GetAllSupplierPayments(ctx, &pb.GetAllSupplierPaymentsRequest{
		Page:            req.Page,
		PageSize:        req.PageSize,
		SupplierId:      req.SupplierId,
		PurchaseOrderId: req.PurchaseOrderId,
	})
	if err != nil {
		return common.PaginationResult[*supplier.SupplierPaymentSummary]{}, err
	}

	items := make([]*supplier.SupplierPaymentSummary, 0, len(res.Payments))
	for _, p := range res.Payments {
		items = append(items, &supplier.SupplierPaymentSummary{
			Id:                p.Id,
			PurchaseOrderId:   p.PurchaseOrderId,
			PurchaseOrderCode: p.PurchaseOrderCode,
			Code:              p.Code,
			Amount:            p.Amount,
			Method:            p.Method,
			Status:            p.Status,
			PaidAt:            p.PaidAt,
			CreatedAt:         p.CreatedAt,
		})
	}

	totalPages := int32(0)
	if req.PageSize > 0 {
		totalPages = int32(math.Ceil(float64(res.TotalCount) / float64(req.PageSize)))
	}

	return common.PaginationResult[*supplier.SupplierPaymentSummary]{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: totalPages,
	}, nil
}
