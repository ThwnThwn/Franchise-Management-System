package sales

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/withdrawal"
	"context"

	"github.com/google/uuid"
)

type IWithdrawalService interface {
	Create(ctx context.Context, req *withdrawal.CreateWithdrawalReq) error
	GetMyWithdrawals(ctx context.Context, req *withdrawal.GetMyWithdrawalsReq) (
		common.PaginationResult[*withdrawal.WithdrawalSummary], error)
	CancelMyWithdrawal(ctx context.Context, id uuid.UUID, req *withdrawal.CancelMyWithdrawalReq) error

	GetAll(ctx context.Context, req *withdrawal.GetAllWithdrawalsReq) (
		common.PaginationResult[*withdrawal.WithdrawalSummary], error)
	GetById(ctx context.Context, id uuid.UUID) (*withdrawal.WithdrawalDetailRes, error)

	Approve(ctx context.Context, id uuid.UUID, req *withdrawal.ApproveWithdrawalReq) error
	Reject(ctx context.Context, id uuid.UUID, req *withdrawal.RejectWithdrawalReq) error
	Cancel(ctx context.Context, id uuid.UUID, req *withdrawal.CancelWithdrawalReq) error
	Complete(ctx context.Context, id uuid.UUID, req *withdrawal.CompleteWithdrawalReq) error
}
