package engagement

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/point"
	"context"
)

type IPointLogService interface {
	GetMyPointLog(ctx context.Context, req *point.GetMyPointLogRequest) (
		common.PaginationResult[*point.LogResponse], error)
}
