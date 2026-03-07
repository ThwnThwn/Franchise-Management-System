package supplier

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
)

type CreateSupplierPaymentReq struct {
	PurchaseOrderId string                     `json:"purchaseOrderId" binding:"required"`
	Method          enum.SupplierPaymentMethod `json:"method"`
	ReturnUrl       *string                    `json:"returnUrl"`
}

type GetAllSupplierPaymentsReq struct {
	common.PaginationQuery

	SupplierId      *string `form:"supplierId"`
	PurchaseOrderId *string `form:"purchaseOrderId"`
}
