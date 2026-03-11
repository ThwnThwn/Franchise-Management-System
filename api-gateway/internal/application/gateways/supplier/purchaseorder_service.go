package supplier

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
	"context"

	"github.com/google/uuid"
)

type IPurchaseOrderService interface {
	GetAll(ctx context.Context, req *supplier.GetPurchaseOrdersReq) (common.PaginationResult[*supplier.PurchaseOrderSummary], error)
	GetById(ctx context.Context, id uuid.UUID) (*supplier.PurchaseOrderDetail, error)

	Create(ctx context.Context, req *supplier.CreatePurchaseOrderReq) (*supplier.PurchaseOrderBaseRes, error)
	Update(ctx context.Context, id uuid.UUID, req *supplier.UpdatePurchaseOrderReq) (*supplier.PurchaseOrderBaseRes, error)
	Cancel(ctx context.Context, id uuid.UUID, req *supplier.CancelPurchaseOrderReq) (*supplier.PurchaseOrderBaseRes, error)
	Complete(ctx context.Context, id uuid.UUID) (*supplier.PurchaseOrderBaseRes, error)
	GetLatestPrices(ctx context.Context, ingredientIds []string) (map[string]float64, error)
}
