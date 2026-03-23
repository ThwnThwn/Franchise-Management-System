package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/mapper"
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

// GetAll implements franchise.IShiftSwapService.
func (s *shiftSwapGrpcClient) GetAll(ctx context.Context, req *shift.GetAllSwapsParams) ([]*shift.SwapRequestItem, error) {
	var statusVal int32 = -1
	if req.Status != nil {
		statusVal = *req.Status
	}

	res, err := s.client.GetSwapRequestsByStore(ctx, &franchisePb.GetSwapRequestsByStoreRequest{
		StoreId: stringutils.GetString(req.StoreId),
		Status:  statusVal,
	})

	if err != nil {
		return []*shift.SwapRequestItem{}, err
	}

	items := make([]*shift.SwapRequestItem, 0, len(res.SwapRequests))
	for _, item := range res.SwapRequests {
		items = append(items, mapper.MapShiftSwapRequestItem(item))
	}

	return items, nil
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
		items = append(items, mapper.MapShiftSwapRequestItem(item))
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
