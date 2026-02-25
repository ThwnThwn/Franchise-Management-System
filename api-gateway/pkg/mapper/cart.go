package mapper

import (
	"api-gateway/internal/domain/models/cart"
	"api-gateway/pkg/pb/sales"
)

func MapCartItemReplyPbToModel(c *sales.CartItemReply) *cart.CartItemReply {
	return &cart.CartItemReply{
		Id:          c.Id,
		Sku:         c.Sku,
		ProductName: c.ProductName,
		CustomerId:  c.CustomerId,
		Quantity:    c.Quantity,
		BasePrice:   c.BasePrice,
	}
}
