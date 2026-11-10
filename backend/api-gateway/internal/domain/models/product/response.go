package product

type GetProductsReply struct {
	Products   []*Product `json:"products"`
	TotalCount int32      `json:"totalCount"`
}

type Product struct {
	Id           string            `json:"id"`
	Name         string            `json:"name"`
	Description  string            `json:"description"`
	ImageUrl     string            `json:"imageUrl"`
	IsActive     bool              `json:"isActive"`
	CategoryId   string            `json:"categoryId"`
	CategoryName string            `json:"categoryName"`
	Variants     []*ProductVariant `json:"variants"`
}

type ProductVariant struct {
	Id         string  `json:"id"`
	Sku        string  `json:"sku"`
	Name       string  `json:"name"`
	Attributes string  `json:"attributes"`
	BasePrice  float64 `json:"basePrice"`
	Cost       float64 `json:"cost"`
	IsActive   bool    `json:"isActive"`
}

type ProductVariantDetail struct {
	ProductId   string  `json:"productId"`
	VariantId   string  `json:"variantId"`
	ProductName string  `json:"productName"`
	Sku         string  `json:"sku"`
	Attributes  string  `json:"attributes"`
	BasePrice   float64 `json:"basePrice"`
	ImageUrl    string  `json:"imageUrl"`
}
