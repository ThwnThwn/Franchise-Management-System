package product

import "github.com/google/wire"

type ProductHandlers struct {
	*ProductHandler
	*CategoryHandler
}

func NewProductHandlers(
	product *ProductHandler,
	cate *CategoryHandler,
) ProductHandlers {
	return ProductHandlers{
		ProductHandler:  product,
		CategoryHandler: cate,
	}
}

var ProviderSet = wire.NewSet(
	NewProductHandler,
	NewCategoryHandler,
	NewProductHandlers,
)
