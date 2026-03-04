package sales

import (
	"api-gateway/config"
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/order"
	salesPb "api-gateway/pkg/pb/sales"
	"context"

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
