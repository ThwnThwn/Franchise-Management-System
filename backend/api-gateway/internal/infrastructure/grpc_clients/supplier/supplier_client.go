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

type supplierGrpcClient struct {
	client pb.SupplierGrpcClient
}

func NewSupplierClient(f *clientFactory) gw.ISupplierService {
	return &supplierGrpcClient{client: pb.NewSupplierGrpcClient(f.conn)}
}

func (s *supplierGrpcClient) GetAll(ctx context.Context, req *supplier.GetSuppliersReq) (common.PaginationResult[*supplier.SupplierSummary], error) {
	grpcReq := &pb.GetSuppliersRequest{
		Code:     req.Code,
		IsActive: req.IsActive,
		Page:     req.Page,
		PageSize: req.PageSize,
		Name:     req.Name,
	}

	res, err := s.client.GetSuppliers(ctx, grpcReq)
	if err != nil {
		return common.PaginationResult[*supplier.SupplierSummary]{}, err
	}

	items := make([]*supplier.SupplierSummary, 0, len(res.Suppliers))
	for _, item := range res.Suppliers {
		items = append(items, &supplier.SupplierSummary{
			Id:            item.Id,
			Code:          item.Code,
			Name:          item.Name,
			ContactPerson: item.ContactPerson,
			Phone:         item.Phone,
			IsActive:      item.IsActive,
		})
	}

	totalPages := int32(0)
	if req.PageSize > 0 {
		totalPages = int32(math.Ceil(float64(res.TotalCount) / float64(req.PageSize)))
	}

	return common.PaginationResult[*supplier.SupplierSummary]{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: totalPages,
	}, nil
}

// GetById: Map dữ liệu chi tiết (nhúng Summary)
func (s *supplierGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*supplier.SupplierDetail, error) {
	res, err := s.client.GetSupplierById(ctx, &pb.GetSupplierByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}

	return &supplier.SupplierDetail{
		SupplierSummary: supplier.SupplierSummary{
			Id:            res.Id,
			Code:          res.Code,
			Name:          res.Name,
			ContactPerson: res.ContactPerson,
			Phone:         res.Phone,
			IsActive:      res.IsActive,
		},
		Email:     res.Email,
		Address:   res.Address,
		TaxCode:   res.TaxCode,
		CreatedAt: res.CreatedAt,
		UpdatedAt: res.UpdatedAt,
	}, nil
}

// Create
func (s *supplierGrpcClient) Create(ctx context.Context, req *supplier.CreateSupplierReq) (*supplier.SupplierBaseRes, error) {
	res, err := s.client.CreateSupplier(ctx, &pb.CreateSupplierRequest{
		Code:          req.Code,
		Name:          req.Name,
		ContactPerson: req.ContactPerson,
		Phone:         req.Phone,
		Email:         req.Email,
		Address:       req.Address,
		TaxCode:       req.TaxCode,
	})
	if err != nil {
		return nil, err
	}

	return &supplier.SupplierBaseRes{
		Id:      res.Id,
		Code:    res.Code,
		Message: res.Message,
	}, nil
}

// Update: Nhận Id riêng từ Path
func (s *supplierGrpcClient) Update(ctx context.Context, id uuid.UUID, req *supplier.UpdateSupplierReq) (*supplier.SupplierBaseRes, error) {
	res, err := s.client.UpdateSupplier(ctx, &pb.UpdateSupplierRequest{
		Id:            id.String(), // Lấy Id từ Path
		Code:          req.Code,
		Name:          req.Name,
		ContactPerson: req.ContactPerson,
		Phone:         req.Phone,
		Email:         req.Email,
		Address:       req.Address,
		TaxCode:       req.TaxCode,
		IsActive:      req.IsActive,
	})
	if err != nil {
		return nil, err
	}

	return &supplier.SupplierBaseRes{Message: res.Message}, nil
}

// Delete
func (s *supplierGrpcClient) Delete(ctx context.Context, id uuid.UUID) (*supplier.SupplierBaseRes, error) {
	res, err := s.client.DeleteSupplier(ctx, &pb.DeleteSupplierRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}

	return &supplier.SupplierBaseRes{Message: res.Message}, nil
}
