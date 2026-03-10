package sales

import (
	"api-gateway/config"
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/withdrawal"
	"api-gateway/pkg/mapper"
	salesPb "api-gateway/pkg/pb/sales"
	"context"

	"github.com/google/uuid"
)

type withdrawalGrpcClient struct {
	client salesPb.WithdrawalServiceClient
}

// ProvideOrderClient trích xuất Order Client từ Factory
func ProvideWithdrawalClient(f *salesClientFactory, cfg *config.Config) salesGW.IWithdrawalService {
	return &withdrawalGrpcClient{
		client: salesPb.NewWithdrawalServiceClient(f.conn),
	}
}

// Create implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) Create(ctx context.Context, req *withdrawal.CreateWithdrawalReq) error {
	_, err := w.client.CreateWithdrawal(ctx, &salesPb.CreateWithdrawalRequest{
		Amount:        req.Amount,
		BankCode:      req.BankCode,
		BankName:      req.BankName,
		AccountName:   req.AccountName,
		AccountNumber: req.AccountNumber,
		Notes:         req.Notes,
	})
	return err
}

// GetMyWithdrawals implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) GetMyWithdrawals(ctx context.Context, req *withdrawal.GetMyWithdrawalsReq) (common.PaginationResult[*withdrawal.WithdrawalSummary], error) {
	res, err := w.client.GetMyWithdrawals(ctx, &salesPb.GetMyWithdrawalsRequest{
		PageIndex: req.Page,
		PageSize:  req.PageSize,
		Status:    salesPb.WithdrawalStatus(req.Status),
	})
	if err != nil {
		return common.PaginationResult[*withdrawal.WithdrawalSummary]{}, err
	}

	items := make([]*withdrawal.WithdrawalSummary, 0, len(res.Items))
	for _, item := range res.Items {
		items = append(items, mapper.MapWithdrawalSummary(item))
	}

	return common.PaginationResult[*withdrawal.WithdrawalSummary]{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// CancelMyWithdrawal implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) CancelMyWithdrawal(ctx context.Context, id uuid.UUID, req *withdrawal.CancelMyWithdrawalReq) error {
	_, err := w.client.CancelMyWithdrawal(ctx, &salesPb.CancelMyWithdrawalRequest{
		RequestId:          id.String(),
		CancellationReason: req.CancellationReason,
	})
	return err
}

// GetAll implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) GetAll(ctx context.Context, req *withdrawal.GetAllWithdrawalsReq) (common.PaginationResult[*withdrawal.WithdrawalSummary], error) {
	res, err := w.client.GetAllWithdrawals(ctx, &salesPb.GetAllWithdrawalsRequest{
		UserId:    req.UserId,
		Status:    salesPb.WithdrawalStatus(req.Status),
		PageIndex: req.Page,
		PageSize:  req.PageSize,
	})

	if err != nil {
		return common.PaginationResult[*withdrawal.WithdrawalSummary]{}, err
	}

	items := make([]*withdrawal.WithdrawalSummary, 0, len(res.Items))
	for _, item := range res.Items {
		items = append(items, mapper.MapWithdrawalSummary(item))
	}

	return common.PaginationResult[*withdrawal.WithdrawalSummary]{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// GetById implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*withdrawal.WithdrawalDetailRes, error) {
	res, err := w.client.GetWithdrawalById(ctx, &salesPb.GetWithdrawalByIdRequest{
		RequestId: id.String(),
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapWithdrawalDetail(res), nil
}

// Reject implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) Approve(ctx context.Context, id uuid.UUID, req *withdrawal.ApproveWithdrawalReq) error {
	_, err := w.client.ApproveWithdrawal(ctx, &salesPb.ApproveWithdrawalRequest{
		RequestId:     id.String(),
		ApprovalNotes: req.ApprovalNotes,
	})
	return err
}

// Reject implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) Reject(ctx context.Context, id uuid.UUID, req *withdrawal.RejectWithdrawalReq) error {
	_, err := w.client.RejectWithdrawal(ctx, &salesPb.RejectWithdrawalRequest{
		RequestId:       id.String(),
		RejectionReason: req.RejectionReason,
	})
	return err
}

// Cancel implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) Cancel(ctx context.Context, id uuid.UUID, req *withdrawal.CancelWithdrawalReq) error {
	_, err := w.client.CancelWithdrawal(ctx, &salesPb.CancelWithdrawalRequest{
		RequestId:          id.String(),
		CancellationReason: req.CancellationReason,
	})
	return err
}

// Complete implements sales.IWithdrawalService.
func (w *withdrawalGrpcClient) Complete(ctx context.Context, id uuid.UUID, req *withdrawal.CompleteWithdrawalReq) error {
	_, err := w.client.CompleteWithdrawal(ctx, &salesPb.CompleteWithdrawalRequest{
		RequestId:         id.String(),
		ReceiptFile:       req.ReceiptFile,
		BankTransactionId: req.BankTransactionId,
	})
	return err
}
