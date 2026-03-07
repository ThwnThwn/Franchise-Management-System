package sales

import (
	"api-gateway/config"
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/order"
	salesPb "api-gateway/pkg/pb/sales"
	"context"
	"strings"

	"github.com/google/uuid"
)

type paymentGrpcClient struct {
	client salesPb.PaymentGrpcClient
	cfg    *config.Config
}

// ProvideOrderClient trích xuất Order Client từ Factory
func ProvidePaymentClient(f *salesClientFactory, cfg *config.Config) salesGW.IPaymentService {
	return &paymentGrpcClient{
		client: salesPb.NewPaymentGrpcClient(f.conn),
		cfg:    cfg,
	}
}

// CreatePaymentUrl implements sales.IOrderService.
func (o *paymentGrpcClient) CreatePaymentUrl(ctx context.Context,
	id uuid.UUID, req *order.CreatePaymentRequest,
) (string, error) {
	var returnUrl string
	if req.ReturnUrl != nil {
		returnUrl = *req.ReturnUrl
	} else {
		returnUrl = o.cfg.VNPay.ClientReturnUrl
	}

	res, err := o.client.CreatePayment(ctx, &salesPb.CreatePaymentRequest{
		OrderId:       id.String(),
		ReturnUrl:     &returnUrl,
		PaymentMethod: salesPb.PaymentMethodType(req.PaymentMethod),
	})
	if err != nil {
		return "", err
	}

	return res.Url, nil
}

// GetAll: Lấy toàn bộ danh sách payment (Admin)
func (o *paymentGrpcClient) GetAll(ctx context.Context, req *order.GetAllPaymentsReq) (common.PaginationResult[*order.PaymentSummary], error) {
	res, err := o.client.GetAllPayments(ctx, &salesPb.GetAllPaymentsRequest{
		Page:     req.Page,
		PageSize: req.PageSize,
	})
	if err != nil {
		return common.PaginationResult[*order.PaymentSummary]{}, err
	}

	items := make([]*order.PaymentSummary, 0, len(res.Payments))
	for _, p := range res.Payments {
		items = append(items, &order.PaymentSummary{
			Id:        p.Id,
			OrderId:   p.OrderId,
			Code:      p.Code,
			Amount:    p.Amount,
			Currency:  p.Currency,
			Method:    enum.PaymentMethod(p.Method), // Ép kiểu trực tiếp từ int32
			Status:    p.Status,
			PaidAt:    p.PaidAt,
			CreatedAt: p.CreatedAt,
		})
	}

	return common.PaginationResult[*order.PaymentSummary]{
		Items:      items,
		TotalCount: int32(res.TotalCount),
		TotalPages: res.TotalPages, // Bốc trực tiếp từ Proto .NET trả về
	}, nil
}

// GetMyHistory: Lịch sử thanh toán cho Customer Dashboard
func (o *paymentGrpcClient) GetMyHistory(ctx context.Context, req *order.GetMyPaymentHistoryReq) (common.PaginationResult[*order.PaymentHistoryItem], error) {
	res, err := o.client.GetMyPaymentHistory(ctx, &salesPb.GetMyPaymentHistoryRequest{
		Page:      req.Page,
		PageSize:  req.PageSize,
		StartDate: req.StartDate,
		EndDate:   req.EndDate,
		Status:    req.Status,
	})
	if err != nil {
		return common.PaginationResult[*order.PaymentHistoryItem]{}, err
	}

	items := make([]*order.PaymentHistoryItem, 0, len(res.Items))
	for _, item := range res.Items {
		items = append(items, &order.PaymentHistoryItem{
			Id:            item.Id,
			OrderId:       item.OrderId,
			OrderCode:     item.OrderCode,
			Amount:        item.Amount,
			Currency:      item.Currency,
			PaymentMethod: toPaymentMethodEnum(item.PaymentMethod), // Map từ string sang Enum
			Status:        item.Status,
			PaymentDate:   item.PaymentDate,
		})
	}

	return common.PaginationResult[*order.PaymentHistoryItem]{
		Items:      items,
		TotalCount: int32(res.TotalCount),
		TotalPages: res.TotalPages,
	}, nil
}

// ProcessVnPayReturn implements sales.IPaymentService.
func (o *paymentGrpcClient) ProcessVnPayReturn(ctx context.Context, queryParams map[string]string) (bool, string, error) {
	res, err := o.client.ProcessVnPayReturn(ctx, &salesPb.VnPayReturnRequest{
		QueryParams: queryParams,
	})
	if err != nil {
		return false, "", err
	}

	return res.IsSuccess, res.Message, nil
}

// ProcessVnPayIpn implements sales.IPaymentService.
func (o *paymentGrpcClient) ProcessVnPayIpn(ctx context.Context, queryParams map[string]string) (string, string, error) {
	res, err := o.client.ProcessVnPayIpn(ctx, &salesPb.VnPayIpnRequest{
		QueryParams: queryParams,
	})
	if err != nil {
		return "", "", err
	}

	return res.RspCode, res.Message, nil
}

// Helper để map string từ gRPC sang Enum Domain
func toPaymentMethodEnum(methodStr string) enum.PaymentMethod {
	switch strings.ToLower(methodStr) {
	case "vnpay":
		return enum.PaymentMethod_VNPAY
	default:
		return enum.PaymentMethod_CASH
	}
}
