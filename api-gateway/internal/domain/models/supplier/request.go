package supplier

import "api-gateway/internal/domain/models/common"

type CreateSupplierReq struct {
	Code          string  `json:"code" binding:"required"`
	Name          string  `json:"name" binding:"required"`
	ContactPerson *string `json:"contactPerson"`
	Phone         *string `json:"phone"`
	Email         *string `json:"email"`
	Address       *string `json:"address"`
	TaxCode       *string `json:"taxCode"`
}

type UpdateSupplierReq struct {
	Code          string  `json:"code" binding:"required"`
	Name          *string `json:"name"`
	ContactPerson *string `json:"contactPerson"`
	Phone         *string `json:"phone"`
	Email         *string `json:"email"`
	Address       *string `json:"address"`
	TaxCode       *string `json:"taxCode"`
	IsActive      *bool   `json:"isActive"`
}

type GetSuppliersReq struct {
	common.PaginationQuery

	Code     *string `form:"code"` // GET dùng form tag cho Query String
	IsActive *bool   `form:"isActive"`
	Name     *string `form:"name"`
}
