package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	franchisePb "api-gateway/pkg/pb/franchise"
	"api-gateway/pkg/utils/stringutils"
	"context"

	"github.com/google/uuid"
)

type shiftSwapGrpcClient struct {
	client franchisePb.ShiftSwapGrpcServiceClient
}

func ProvideShiftSwapClient(f *clientFactory) franchiseGW.IShiftSwapService {
	return &shiftSwapGrpcClient{client: franchisePb.NewShiftSwapGrpcServiceClient(f.conn)}
}

// ApproveSwap implements franchise.IShiftSwapService.
func (s *shiftSwapGrpcClient) ApproveSwap(ctx context.Context,
	swapId uuid.UUID, managerId uuid.UUID, req *shift.ApproveSwapRequest,
) error {
	_, err := s.client.ApproveSwap(ctx, &franchisePb.ApproveSwapRequest{
		SwapRequestId: swapId.String(),
		ReviewedBy:    managerId.String(),
		Approve:       req.Approve,
		RejectReason:  stringutils.GetString(req.RejectReason),
	})
	return err
}

// CancelSwap implements franchise.IShiftSwapService.
func (s *shiftSwapGrpcClient) CancelSwap(ctx context.Context, swapId uuid.UUID, userId uuid.UUID) error {
	_, err := s.client.CancelSwap(ctx, &franchisePb.CancelSwapRequest{
		SwapRequestId: swapId.String(),
		UserId:        userId.String(),
	})

	return err
}

// GetMySwaps implements franchise.IShiftSwapService.
func (s *shiftSwapGrpcClient) GetMySwaps(ctx context.Context, userId uuid.UUID) ([]*shift.SwapRequestItem, error) {
	res, err := s.client.GetMySwapRequests(ctx, &franchisePb.GetMySwapRequestsRequest{
		UserId: userId.String(),
	})
	if err != nil {
		return []*shift.SwapRequestItem{}, err
	}

	items := make([]*shift.SwapRequestItem, 0, len(res.SwapRequests))
	for _, item := range res.SwapRequests {
		items = append(items, &shift.SwapRequestItem{
			Id: item.Id,

			SourceUserId:   item.SourceUserId,
			SourceUserName: item.SourceUserName,
			TargetUserId:   item.TargetUserId,
			TargetUserName: item.TargetUserName,

			Status:       item.Status,
			Reason:       stringutils.ToPtr(item.Reason),
			RejectReason: stringutils.ToPtr(item.RejectReason),

			ExpiresAt: item.ExpiresAt,
			CreatedAt: item.CreatedAt,

			ShiftId:   item.ShiftId,
			ShiftDate: item.ShiftDate,
			StartTime: item.StartTime,
			EndTime:   item.EndTime,
		})
	}

	return items, nil
}

// Release implements franchise.IShiftSwapService.
func (s *shiftSwapGrpcClient) Release(ctx context.Context, assignmentId uuid.UUID, userId uuid.UUID) error {
	_, err := s.client.ReleaseShift(ctx, &franchisePb.ReleaseShiftRequest{
		AssignmentId: assignmentId.String(),
		UserId:       userId.String(),
	})

	return err
}

// RequestSwap implements franchise.IShiftSwapService.
func (s *shiftSwapGrpcClient) RequestSwap(ctx context.Context,
	assignmentId uuid.UUID, sourceUserId uuid.UUID, req *shift.RequestSwapRequest,
) error {
	_, err := s.client.RequestSwap(ctx, &franchisePb.RequestSwapRequest{
		AssignmentId: assignmentId.String(),
		SourceUserId: sourceUserId.String(),
		TargetUserId: req.TargetUserId,
		Reason:       req.Reason,
	})

	return err
}

// RespondSwap implements franchise.IShiftSwapService.
func (s *shiftSwapGrpcClient) RespondSwap(ctx context.Context,
	swapId uuid.UUID, userId uuid.UUID, req *shift.RespondSwapRequest,
) error {
	_, err := s.client.RespondSwap(ctx, &franchisePb.RespondSwapRequest{
		SwapRequestId: swapId.String(),
		UserId:        userId.String(),
		Accept:        req.Accept,
		RejectReason:  stringutils.GetString(req.RejectReason),
	})

	return err
}
