package sales

import (
	"api-gateway/global"
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/mapper"
	salesPb "api-gateway/pkg/pb/sales"
	"api-gateway/pkg/utils/numberutils"
	"api-gateway/pkg/utils/timeutils"
	"context"

	"github.com/google/uuid"
)

type OrderGrpcClient struct {
	client        salesPb.OrderGrpcClient
	paymentClient salesPb.PaymentGrpcClient
}

// ProvideOrderClient trích xuất Order Client từ Factory
func ProvideOrderClient(f *salesClientFactory) salesGW.IOrderService {
	return &OrderGrpcClient{
		client:        salesPb.NewOrderGrpcClient(f.conn),
		paymentClient: salesPb.NewPaymentGrpcClient(f.conn),
	}
}

// CreateOrder implements sales.IOrderService.
func (o *OrderGrpcClient) CreateOrder(ctx context.Context, req *order.CreateOrderRequest) (*order.OrderReply, error) {
	pbItems := []*salesPb.CartItem{}

	for _, item := range req.Items {
		pbItems = append(pbItems, &salesPb.CartItem{
			Sku:      item.Sku,
			Quantity: item.Quantity,
		})
	}

	res, err := o.client.CreateOrder(ctx, &salesPb.CreateOrderRequest{
		Items:   pbItems,
		Notes:   req.Notes,
		StoreId: req.StoreId,
	})
	if err != nil {
		return nil, err
	}

	return &order.OrderReply{
		OrderId:           res.OrderId,
		OrderCode:         res.OrderCode,
		TotalAmount:       res.TotalAmount,
		CustomerId:        res.CustomerId,
		CheckStockRespone: mapper.MapCheckEnoughStockRes(res.CheckStockRespone),
	}, nil
}

// UpdateOrder implements sales.IOrderService.
func (o *OrderGrpcClient) UpdateOrder(ctx context.Context, id uuid.UUID, items []*order.UpdateOrderItem) (*order.OrderReply, error) {
	pbItems := []*salesPb.UpdateOrderItem{}
	for _, i := range items {
		pbItems = append(pbItems, &salesPb.UpdateOrderItem{
			Sku:      i.Sku,
			Quantity: i.Quantity,
		})
	}

	res, err := o.client.UpdateOrder(ctx, &salesPb.UpdateOrderRequest{
		OrderId: id.String(),
		Items:   pbItems,
	})
	if err != nil {
		return nil, err
	}

	return &order.OrderReply{
		OrderId:           res.OrderId,
		OrderCode:         res.OrderCode,
		TotalAmount:       res.TotalAmount,
		CustomerId:        res.CustomerId,
		CheckStockRespone: mapper.MapCheckEnoughStockRes(res.CheckStockRespone),
	}, nil
}

// GetOrderById implements sales.IOrderService.
func (o *OrderGrpcClient) GetOrderById(ctx context.Context, id uuid.UUID) (*order.Order, error) {
	res, err := o.client.GetOrderById(ctx, &salesPb.GetOrderByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}
	items := []*order.OrderItem{}
	for _, item := range res.Order.Items {
		items = append(items, &order.OrderItem{
			Id:          item.Id,
			ProductId:   item.ProductId,
			VariantId:   item.VariantId,
			Quantity:    item.Quantity,
			UnitPrice:   item.UnitPrice,
			ProductName: item.ProductName,
			Sku:         item.Sku,
			ImageUrl:    item.ImageUrl,
		})
	}

	order := &order.Order{
		OrderHeader: order.OrderHeader{
			Id:            res.Order.Id,
			Code:          res.Order.Code,
			Subtotal:      res.Order.Subtotal,
			Tax:           numberutils.GetFloat64(res.Order.Tax),
			TotalAmount:   res.Order.TotalAmount,
			PaidAmount:    res.Order.PaidAmount,
			PaymentMethod: numberutils.GetInt32(res.Order.PaymentMethod),
			Status:        numberutils.GetInt32(res.Order.Status),
			PaidAt:        timeutils.StringToTime(res.Order.PaidAt),
			Notes:         res.Order.Notes,
			CreatedAt:     timeutils.StringToTime(res.Order.CreatedAt),
			UpdatedAt:     timeutils.StringToTime(res.Order.UpdatedAt),
			CustomerId:    res.Order.CustomerId,
		},
		Items: items,
	}

	return order, nil
}

// GetMyOrders implements sales.IOrderService.
func (o *OrderGrpcClient) GetMyOrders(ctx context.Context, req *order.GetMyOrdersRequest) ([]*order.OrderSummary, error) {
	res, err := o.client.GetMyOrders(ctx, &salesPb.GetMyOrdersRequest{
		Page:     &req.Page,
		PageSize: &req.PageSize,
	})
	if err != nil {
		return nil, err
	}

	orders := make([]*order.OrderSummary, 0, len(res.Orders))
	for _, o := range res.Orders {
		orders = append(orders, &order.OrderSummary{
			OrderHeader: order.OrderHeader{
				Id:            o.Id,
				Code:          o.Code,
				Subtotal:      o.Subtotal,
				Tax:           numberutils.GetFloat64(o.Tax),
				TotalAmount:   o.TotalAmount,
				PaidAmount:    numberutils.GetFloat64(o.PaidAmount),
				PaymentMethod: numberutils.GetInt32(o.PaymentMethod),
				Status:        numberutils.GetInt32(o.Status),
				PaidAt:        timeutils.StringToTime(o.PaidAt),
				Notes:         o.Notes,
				CreatedAt:     timeutils.StringToTime(o.CreatedAt),
				UpdatedAt:     timeutils.StringToTime(o.UpdatedAt),

				CustomerId: o.CustomerId,
			},
		})
	}

	return orders, nil
}

// UpdateStatus implements sales.IOrderService.
func (o *OrderGrpcClient) UpdateStatus(ctx context.Context, id uuid.UUID, status enum.OrderStatus) error {
	_, err := o.client.UpdateOrderStatus(ctx, &salesPb.UpdateOrderStatusRequest{
		OrderId: id.String(),
		Status:  salesPb.OrderStatus(status),
	})

	return err
}

// CreatePaymentUrl implements sales.IOrderService.
func (o *OrderGrpcClient) CreatePaymentUrl(ctx context.Context,
	id uuid.UUID, req *order.CreatePaymentRequest,
) (string, error) {
	var returnUrl string
	if req.ReturnUrl != nil {
		returnUrl = *req.ReturnUrl
	} else {
		returnUrl = global.Config.FrontEndOrigin + "/orders"
	}

	res, err := o.paymentClient.CreatePayment(ctx, &salesPb.CreatePaymentRequest{
		OrderId:       id.String(),
		ReturnUrl:     &returnUrl,
		PaymentMethod: salesPb.PaymentMethodType(req.PaymentMethod),
	})
	if err != nil {
		return "", err
	}

	return res.Url, nil
}
