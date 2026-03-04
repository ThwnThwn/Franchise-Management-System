package engagement

import (
	engagementGW "api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/point"
	"context"

	engagementPb "api-gateway/pkg/pb/engagement"
	"api-gateway/pkg/utils/timeutils"

	"github.com/google/uuid"
)

type pointLogGrpcClient struct {
	client engagementPb.PointLogGrpcClient
}

func ProvidePointLogClient(f *clientFactory) engagementGW.IPointLogService {
	return &pointLogGrpcClient{client: engagementPb.NewPointLogGrpcClient(f.conn)}
}

// GetMyPointLog implements engagement.IPointLogService.
func (p *pointLogGrpcClient) GetMyPointLog(ctx context.Context, userId uuid.UUID, req *point.GetMyPointLogRequest) (common.PaginationResult[*point.LogResponse], error) {
	res, err := p.client.GetMyPointLog(ctx, &engagementPb.GetMyPointLogRequest{
		Id:       userId.String(),
		FromDate: timeutils.ParseStringPtrToTimestamp(req.FromDate),
		ToDate:   timeutils.ParseStringPtrToTimestamp(req.ToDate),
		Page:     req.Page,
		PageSize: req.PageSize,
	})

	if err != nil {
		return common.PaginationResult[*point.LogResponse]{}, err
	}

	items := make([]*point.LogResponse, 0, len(res.LogRes))
	for _, item := range res.LogRes {
		items = append(items, &point.LogResponse{
			Id:            item.Id,
			UserId:        item.UserId,
			Amount:        item.Amount,
			BalanceBefore: item.BalanceBefore,
			BalanceAfter:  item.BalanceAfter,
			ActionType:    item.ActionType,
			ReasonCode:    item.ReasonCode,
			ReferenceId:   item.ReferenceId,
			Description:   item.Description,
			ExpiredAt:     item.ExpiredAt,
			CreatedAt:     item.CreatedAt,
		})
	}

	return common.PaginationResult[*point.LogResponse]{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}
