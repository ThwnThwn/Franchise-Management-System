package recipe

type GetAllReq struct {
	VariantIds []string `form:"variantIds"`
}

type RecipeIngredientRequest struct {
	IngredientId string  `json:"ingredientId"`
	Quantity     float64 `json:"quantity"`
}
