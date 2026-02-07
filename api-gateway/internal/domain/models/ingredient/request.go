package ingredient

import "api-gateway/internal/domain/models/common"

type IngredientStockItem struct {
	IngredientId string  `json:"ingredientId"`
	Quantity     float64 `json:"quantity"`
}

// type IncreaseStockRequest struct {
// 	StoreId string                 `json:"storeId"`
// 	Items   []*IngredientStockItem `json:"items"`
// }

type GetAllStockReq struct {
	common.PaginationQuery
	// StoreIds      []string `form:"storeIds"`
	CategoryIds   []string `form:"categoryIds"`
	StockStatus   *int32   `form:"stockStatus"`
	IngredientIds []string `form:"ingredientIds"`
}
