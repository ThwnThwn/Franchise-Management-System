package product

import "github.com/google/uuid"

type GetProductsRequest struct {
	CategoryId uuid.UUID `json:"categoryId"`
}
