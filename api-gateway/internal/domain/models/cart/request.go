package cart

type UpsertCartItemRequest struct {
	Sku      string `json:"sku"`
	Quantity int32  `json:"quantity"`
}

type DeleteCartItemsRequest struct {
	Skus []string `json:"skus"`
}
