package sales

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/wallet"
	"context"

	"github.com/google/uuid"
)

type IRefundTicketService interface {
	GetAll(ctx context.Context, userId *uuid.UUID, req *wallet.GetAllRefundsRequest) (
		common.PaginationResult[*wallet.RefundTicketSummary], error)
	GetById(ctx context.Context, ctxUserId, ticketId uuid.UUID) (*wallet.RefundTicketDetail, error)

	CreateRefundByCustomer(ctx context.Context, req *wallet.CreateRefundByCustomerRequest) error

	Approve(ctx context.Context, ticketId uuid.UUID, req *wallet.ApproveRefundRequest) error
	Reject(ctx context.Context, ticketId uuid.UUID, req *wallet.RejectRefundRequest) error
	Cancel(ctx context.Context, ticketId uuid.UUID, req *wallet.CancelRefundRequest) error
	SetComplete(ctx context.Context, ticketId uuid.UUID, req *wallet.SetCompleteRefundRequest) error
}
