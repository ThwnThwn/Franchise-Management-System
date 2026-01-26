package product

type GetProductsReply struct {
	Products   []*Product `json:"products"`
	TotalCount int32      `json:"totalCount"`
}
type Product struct {
	Id           string         `json:"id"`
	Name         string         `json:"name"`
	Description  string         `json:"description"`
	ImageUrl     string         `json:"imageUrl"`
	IsActive     bool           `json:"isActive"`
	CategoryId   string         `json:"categoryId"`
	CategoryName string         `json:"categoryName"`
	Sizes        []*ProductSize `json:"sizes"`
}

type ProductSize struct {
	Id         string  `json:"id"`
	Sku        string  `json:"sku"`
	Name       string  `json:"name"`
	Attributes string  `json:"attributes"`
	BasePrice  float64 `json:"basePrice"`
	IsActive   bool    `json:"isActive"`
}
