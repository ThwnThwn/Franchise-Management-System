package recipe

type RecipeViewRes struct {
	VariantId string  `json:"variantId"`
	Quantity  float64 `json:"quantity"`
	Name      string  `json:"name"`
	BaseUnit  string  `json:"baseUnit"`
}
