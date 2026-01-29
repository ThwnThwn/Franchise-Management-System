package ingredient

type IngredientStockItem struct {
	IngredientId string  `json:"ingredientId"`
	Quantity     float64 `json:"quantity"`
}

type IncreaseStockRequest struct {
	StoreId string                 `json:"storeId"`
	Items   []*IngredientStockItem `json:"items"`
}
