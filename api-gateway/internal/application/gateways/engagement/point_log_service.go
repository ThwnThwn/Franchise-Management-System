package engagement

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/point"
	"context"

	"github.com/google/uuid"
)

type IPointLogService interface {
	GetMyPointLog(ctx context.Context, userId uuid.UUID, req *point.GetMyPointLogRequest) (
		common.PaginationResult[*point.LogResponse], error)
}
