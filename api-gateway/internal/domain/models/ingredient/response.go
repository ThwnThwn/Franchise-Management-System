package ingredient

type StockViewRes struct {
	StoreId           string             `json:"storeId"`
	IngredientViewRes *IngredientViewRes `json:"ingredientViewRes"`
	Quantity          string             `json:"quantity"`
	MinThreshold      string             `json:"minThreshold"`
	StockStatus       int32              `json:"stockStatus"`
}

type IngredientViewRes struct {
	Id         string `json:"id"`
	Name       string `json:"name"`
	BaseUnit   string `json:"baseUnit"`
	CategoryId string `json:"categoryId"`
}
