package point

import (
	"api-gateway/internal/domain/models/common"
)

type GetMyPointLogRequest struct {
	common.PaginationQuery
	FromDate *string `form:"fromDate"`
	ToDate   *string `form:"toDate"`
}
