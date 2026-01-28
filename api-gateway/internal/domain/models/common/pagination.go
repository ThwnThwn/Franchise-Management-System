package common

type PaginationQuery struct {
	Page     *int32 `form:"page,default=1"`
	PageSize *int32 `form:"pageSize,default=10"`
}
