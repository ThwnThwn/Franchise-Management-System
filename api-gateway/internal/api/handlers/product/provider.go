package product

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewProductHandler,
	NewCategoryHandler,
	// NewProductHandlers,
)

// type ProductHandlers struct {
// 	*ProductHandler
// 	*CategoryHandler
// }

// func NewProductHandlers(
// 	product *ProductHandler,
// 	cate *CategoryHandler,
// ) ProductHandlers {
// 	return ProductHandlers{
// 		ProductHandler:  product,
// 		CategoryHandler: cate,
// 	}
// }
