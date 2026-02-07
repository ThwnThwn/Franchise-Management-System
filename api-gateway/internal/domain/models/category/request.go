package category

type CreateCategoryReq struct {
	Name        string `json:"name" binding:"required"`
	Description string `json:"description"`
}

type UpdateCategoryReq struct {
	Name        *string `json:"name"`
	Description *string `json:"description"`
}
