package ingredient

import "api-gateway/internal/domain/models/common"

// stock
type IngredientStockItem struct {
	IngredientId string   `json:"ingredientId"`
	Quantity     float64  `json:"quantity"`
	MinThreshold *float64 `json:"minThreshold,omitempty"`
}

type GetAllStockReq struct {
	common.PaginationQuery
	// StoreIds      []string `form:"storeIds"`
	CategoryIds   []string `form:"categoryIds"`
	StockStatus   *int32   `form:"stockStatus"`
	IngredientIds []string `form:"ingredientIds"`
}

// =======================================
// ingredient
// =======================================
type GetAllIngredientReq struct {
	common.PaginationQuery
	CategoryId *string `form:"categoryId"`
}

type CreateIngredientReq struct {
	Name       string  `json:"name" binding:"required"`
	BaseUnit   string  `json:"baseUnit" binding:"required"`
	CategoryId *string `json:"categoryId"`
}

type UpdateIngredientReq struct {
	Name       *string `json:"name"`
	BaseUnit   *string `json:"baseUnit"`
	CategoryId *string `json:"categoryId"`
}

// =======================================
// ingredient category
// =======================================
type CreateIngreCateReq struct {
	Name        string  `json:"name" binding:"required"`
	Description *string `json:"description"`
}

type UpdateIngreCateReq struct {
	Name        *string `json:"name"`
	Description *string `json:"description"`
}
