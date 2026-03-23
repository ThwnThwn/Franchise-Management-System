package supplier

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
	"context"

	"github.com/google/uuid"
)

type ISupplierService interface {
	GetAll(ctx context.Context, req *supplier.GetSuppliersReq) (common.PaginationResult[*supplier.SupplierSummary], error)
	GetById(ctx context.Context, id uuid.UUID) (*supplier.SupplierDetail, error)

	Create(ctx context.Context, req *supplier.CreateSupplierReq) (*supplier.SupplierBaseRes, error)
	Update(ctx context.Context, id uuid.UUID, req *supplier.UpdateSupplierReq) (*supplier.SupplierBaseRes, error)
	Delete(ctx context.Context, id uuid.UUID) (*supplier.SupplierBaseRes, error)
}
