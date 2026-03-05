package sales

import (
	"api-gateway/config"
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/wallet"
	"api-gateway/pkg/mapper"
	salesPb "api-gateway/pkg/pb/sales"
	"api-gateway/pkg/utils/stringutils"
	"context"

	"github.com/google/uuid"
)

type refundTicketGrpcClient struct {
	client salesPb.RefundTicketGrpcServiceClient
}

// ProvideOrderClient trích xuất Order Client từ Factory
func ProvideRefundTicketClient(f *salesClientFactory, cfg *config.Config) salesGW.IRefundTicketService {
	return &refundTicketGrpcClient{
		client: salesPb.NewRefundTicketGrpcServiceClient(f.conn),
	}
}

// GetAll implements sales.IRefundTicketService.
func (r *refundTicketGrpcClient) GetAll(ctx context.Context, userId *uuid.UUID, req *wallet.GetAllRefundsRequest) (
	common.PaginationResult[*wallet.RefundTicketSummary], error,
) {
	res, err := r.client.GetAllRefunds(ctx, &salesPb.GetAllRefundsRequest{
		UserId: stringutils.UuidPtrToStringPtr(userId),
		Status: (*salesPb.RefundTicketStatus)(req.Status),
		Paging: &salesPb.PagedRequest{
			PageIndex: req.Page,
			PageSize:  req.PageSize,
		},
	})
	if err != nil {
		return common.PaginationResult[*wallet.RefundTicketSummary]{}, err
	}

	items := make([]*wallet.RefundTicketSummary, 0, len(res.Items))
	for _, item := range res.Items {
		items = append(items, mapper.MapRefundTicketSummary(item))
	}

	return common.PaginationResult[*wallet.RefundTicketSummary]{
		Items:      items,
		TotalCount: res.Meta.TotalCount,
		TotalPages: res.Meta.TotalPages,
	}, nil
}

// GetById implements sales.IRefundTicketService.
func (r *refundTicketGrpcClient) GetById(ctx context.Context, ctxUserId uuid.UUID, ticketId uuid.UUID) (
	*wallet.RefundTicketDetail, error,
) {
	res, err := r.client.GetRefundById(ctx, &salesPb.GetRefundByIdRequest{
		RefundTicketId: ticketId.String(),
		UserId:         ctxUserId.String(),
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapRefundTicketDetail(res), nil
}

// CreateRefundByCustomer implements sales.IRefundTicketService.
func (r *refundTicketGrpcClient) CreateRefundByCustomer(ctx context.Context, req *wallet.CreateRefundByCustomerRequest) error {
	_, err := r.client.CustomerCreateRefund(ctx, &salesPb.CustomerCreateRefundRequest{
		Amount: req.Amount,
		Reason: req.Reason,
	})
	return err
}

// Approve implements sales.IRefundTicketService.
func (r *refundTicketGrpcClient) Approve(ctx context.Context, ticketId uuid.UUID, req *wallet.ApproveRefundRequest) error {
	_, err := r.client.ApproveRefund(ctx, &salesPb.ApproveRefundRequest{
		RefundTicketId: ticketId.String(),
		ApprovalNotes:  req.ApprovalNotes,
	})
	return err
}

// Reject implements sales.IRefundTicketService.
func (r *refundTicketGrpcClient) Reject(ctx context.Context, ticketId uuid.UUID, req *wallet.RejectRefundRequest) error {
	_, err := r.client.RejectRefund(ctx, &salesPb.RejectRefundRequest{
		RefundTicketId: ticketId.String(),
		RejectNotes:    req.RejectNotes,
	})
	return err
}

// Cancel implements sales.IRefundTicketService.
func (r *refundTicketGrpcClient) Cancel(ctx context.Context, ticketId uuid.UUID, req *wallet.CancelRefundRequest) error {
	_, err := r.client.CancelRefund(ctx, &salesPb.CancelRefundRequest{
		RefundTicketId:     ticketId.String(),
		CancellationReason: req.CancellationReason,
	})
	return err
}

// SetComplete implements sales.IRefundTicketService.
func (r *refundTicketGrpcClient) SetComplete(ctx context.Context, ticketId uuid.UUID, req *wallet.SetCompleteRefundRequest) error {
	_, err := r.client.SetCompleteRefund(ctx, &salesPb.SetCompleteRefundRequest{
		RefundTicketId: ticketId.String(),
		Status:         salesPb.RefundTicketStatus(enum.RefundTicketStatus_REFUND_STATUS_COMPLETED),
		CompleteNotes:  req.CompleteNotes,
	})
	return err
}
