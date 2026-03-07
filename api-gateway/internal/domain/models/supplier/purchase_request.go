package supplier

import "api-gateway/internal/domain/models/common"

// --- PURCHASE ORDER REQUESTS ---

type CreatePurchaseOrderItemReq struct {
	IngredientId string  `json:"ingredientId" binding:"required"`
	Quantity     float64 `json:"quantity" binding:"required,gt=0"`
}

type CreatePurchaseOrderReq struct {
	SupplierId           string                       `json:"supplierId" binding:"required"`
	StoreId              *string                      `json:"storeId"`
	Notes                *string                      `json:"notes"`
	ReferenceCode        *string                      `json:"referenceCode"`
	ExpectedDeliveryDate *string                      `json:"expectedDeliveryDate"`
	DiscountAmount       float64                      `json:"discountAmount"`
	Items                []CreatePurchaseOrderItemReq `json:"items" binding:"required,gt=0"`
}

type UpdatePurchaseOrderReq struct {
	SupplierId           string                       `json:"supplierId" binding:"required"`
	StoreId              *string                      `json:"storeId"`
	Notes                *string                      `json:"notes"`
	ReferenceCode        *string                      `json:"referenceCode"`
	ExpectedDeliveryDate *string                      `json:"expectedDeliveryDate"`
	DiscountAmount       float64                      `json:"discountAmount"`
	Items                []CreatePurchaseOrderItemReq `json:"items" binding:"required,gt=0"`
}

type GetPurchaseOrdersReq struct {
	common.PaginationQuery
	SupplierId *string `form:"supplierId"`
	StoreId    *string `form:"storeId"`
	Status     *int32  `form:"status"`
}

type CancelPurchaseOrderReq struct {
	Reason *string `json:"reason"`
}

type GetLatestPricesReq struct {
	IngredientIds []string `json:"ingredientIds" binding:"required"`
}
