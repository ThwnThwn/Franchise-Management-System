package supplier

import (
	gw "api-gateway/internal/application/gateways/supplier"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
	pb "api-gateway/pkg/pb/supplier"
	"context"
	"math"

	"github.com/google/uuid"
)

type purchaseOrderGrpcClient struct {
	client pb.PurchaseOrderGrpcClient
}

func NewPurchaseOrderClient(f *clientFactory) gw.IPurchaseOrderService {
	return &purchaseOrderGrpcClient{
		client: pb.NewPurchaseOrderGrpcClient(f.conn),
	}
}

// GetAll: Lấy danh sách PO và tính toán TotalPages
func (s *purchaseOrderGrpcClient) GetAll(ctx context.Context, req *supplier.GetPurchaseOrdersReq) (common.PaginationResult[*supplier.PurchaseOrderSummary], error) {
	res, err := s.client.GetPurchaseOrders(ctx, &pb.GetPurchaseOrdersRequest{
		SupplierId: req.SupplierId,
		StoreId:    req.StoreId,
		Status:     req.Status,
		Page:       req.Page,
		PageSize:   req.PageSize,
	})
	if err != nil {
		return common.PaginationResult[*supplier.PurchaseOrderSummary]{}, err
	}

	// Tối ưu hiệu năng: Cấp phát bộ nhớ 1 lần duy nhất
	items := make([]*supplier.PurchaseOrderSummary, 0, len(res.PurchaseOrders))
	for _, item := range res.PurchaseOrders {
		items = append(items, &supplier.PurchaseOrderSummary{
			Id:            item.Id,
			Code:          item.Code,
			SupplierName:  item.SupplierName,
			Status:        item.Status,
			PaymentStatus: item.PaymentStatus,
			TotalAmount:   item.TotalAmount,
			NetAmount:     item.NetAmount,
			OrderDate:     item.OrderDate,
			CreatedAt:     item.CreatedAt,
		})
	}

	// Tự tính TotalPages cho Gateway
	totalPages := int32(0)
	if req.PageSize > 0 {
		totalPages = int32(math.Ceil(float64(res.TotalCount) / float64(req.PageSize)))
	}

	return common.PaginationResult[*supplier.PurchaseOrderSummary]{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: totalPages,
	}, nil
}

// GetById: Map chi tiết đơn hàng kèm danh sách nguyên liệu (items)
func (s *purchaseOrderGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*supplier.PurchaseOrderDetail, error) {
	res, err := s.client.GetPurchaseOrderById(ctx, &pb.GetPurchaseOrderByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}

	// Map danh sách item detail
	orderItems := make([]supplier.PurchaseOrderItemDetail, 0, len(res.Items))
	for _, item := range res.Items {
		orderItems = append(orderItems, supplier.PurchaseOrderItemDetail{
			Id:           item.Id,
			IngredientId: item.IngredientId,
			Quantity:     item.Quantity,
			UnitPrice:    item.UnitPrice,
			TotalPrice:   item.TotalPrice,
		})
	}

	return &supplier.PurchaseOrderDetail{
		PurchaseOrderSummary: supplier.PurchaseOrderSummary{
			Id:            res.Id,
			Code:          res.Code,
			SupplierName:  res.SupplierName,
			Status:        res.Status,
			PaymentStatus: res.PaymentStatus,
			TotalAmount:   res.TotalAmount,
			NetAmount:     res.NetAmount,
			OrderDate:     res.OrderDate,
			CreatedAt:     res.CreatedAt,
		},
		SupplierId:     res.SupplierId,
		StoreId:        res.StoreId,
		DiscountAmount: res.DiscountAmount,
		PaidAmount:     res.PaidAmount,
		CompletedAt:    res.CompletedAt,
		Notes:          res.Notes,
		Items:          orderItems,
		UpdatedAt:      res.UpdatedAt,
	}, nil
}

// Create: Map từ Domain Request sang gRPC Request (Nested items)
func (s *purchaseOrderGrpcClient) Create(ctx context.Context, req *supplier.CreatePurchaseOrderReq) (*supplier.PurchaseOrderBaseRes, error) {
	// Chuyển đổi list items sang format gRPC
	grpcItems := make([]*pb.CreatePurchaseOrderItemRequest, 0, len(req.Items))
	for _, item := range req.Items {
		grpcItems = append(grpcItems, &pb.CreatePurchaseOrderItemRequest{
			IngredientId: item.IngredientId,
			Quantity:     item.Quantity,
		})
	}

	res, err := s.client.CreatePurchaseOrder(ctx, &pb.CreatePurchaseOrderRequest{
		SupplierId:           req.SupplierId,
		StoreId:              req.StoreId,
		Notes:                req.Notes,
		ReferenceCode:        req.ReferenceCode,
		ExpectedDeliveryDate: req.ExpectedDeliveryDate,
		DiscountAmount:       req.DiscountAmount,
		Items:                grpcItems,
	})
	if err != nil {
		return nil, err
	}

	return &supplier.PurchaseOrderBaseRes{
		Id:   res.Id,
		Code: res.Code,
	}, nil
}

// Update: Nhận Id từ Path và Body từ JSON
func (s *purchaseOrderGrpcClient) Update(ctx context.Context, id uuid.UUID, req *supplier.UpdatePurchaseOrderReq) (*supplier.PurchaseOrderBaseRes, error) {
	grpcItems := make([]*pb.CreatePurchaseOrderItemRequest, 0, len(req.Items))
	for _, item := range req.Items {
		grpcItems = append(grpcItems, &pb.CreatePurchaseOrderItemRequest{
			IngredientId: item.IngredientId,
			Quantity:     item.Quantity,
		})
	}

	res, err := s.client.UpdatePurchaseOrder(ctx, &pb.UpdatePurchaseOrderRequest{
		Id:                   id.String(),
		SupplierId:           req.SupplierId,
		StoreId:              req.StoreId,
		Notes:                req.Notes,
		ReferenceCode:        req.ReferenceCode,
		ExpectedDeliveryDate: req.ExpectedDeliveryDate,
		DiscountAmount:       req.DiscountAmount,
		Items:                grpcItems,
	})
	if err != nil {
		return nil, err
	}

	return &supplier.PurchaseOrderBaseRes{Message: res.Message}, nil
}

// Cancel
func (s *purchaseOrderGrpcClient) Cancel(ctx context.Context, id uuid.UUID, req *supplier.CancelPurchaseOrderReq) (*supplier.PurchaseOrderBaseRes, error) {
	res, err := s.client.CancelPurchaseOrder(ctx, &pb.CancelPurchaseOrderRequest{
		Id:     id.String(),
		Reason: req.Reason,
	})
	if err != nil {
		return nil, err
	}

	return &supplier.PurchaseOrderBaseRes{Message: res.Message}, nil
}

// Complete
func (s *purchaseOrderGrpcClient) Complete(ctx context.Context, id uuid.UUID) (*supplier.PurchaseOrderBaseRes, error) {
	res, err := s.client.CompletePurchaseOrder(ctx, &pb.CompletePurchaseOrderRequest{
		PurchaseOrderId: id.String(),
	})
	if err != nil {
		return nil, err
	}

	return &supplier.PurchaseOrderBaseRes{Message: res.Message}, nil
}

// GetLatestPrices: Trả về map[string]float64
func (s *purchaseOrderGrpcClient) GetLatestPrices(ctx context.Context, ingredientIds []string) (map[string]float64, error) {
	res, err := s.client.GetLatestPriceForIngredients(ctx, &pb.GetLatestPriceForIngredientsRequest{
		IngredientIds: ingredientIds,
	})
	if err != nil {
		return nil, err
	}

	return res.Prices, nil
}
