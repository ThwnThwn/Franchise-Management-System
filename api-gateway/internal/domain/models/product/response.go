package product

import "api-gateway/internal/domain/entity"

type GetProductsReply struct {
	Products   []*entity.Product `json:"products"`
	TotalCount int32             `json:"totalCount"`
}
