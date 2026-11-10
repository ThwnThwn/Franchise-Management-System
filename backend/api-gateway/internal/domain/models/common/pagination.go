package common

type PaginationQuery struct {
	Page     int32 `form:"page,default=1"`
	PageSize int32 `form:"pageSize,default=10"`
}

type PaginationResult[T any] struct {
	Items      []T   `json:"items"`
	TotalCount int32 `json:"totalCount"`
	TotalPages int32 `json:"totalPages"`
}
