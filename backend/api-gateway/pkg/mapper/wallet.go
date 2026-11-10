package mapper

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/wallet"
	pb "api-gateway/pkg/pb/sales"
)

func MapWalletTransactionPbToModel(w *pb.WalletTransactionResponse) *wallet.WalletTransactionResponse {
	if w == nil {
		return nil
	}

	return &wallet.WalletTransactionResponse{
		Id:       w.Id,
		WalletId: w.WalletId,
		UserId:   w.UserId,
		OrderId:  w.OrderId,

		InitiatedBy:         w.InitiatedBy,
		ProcessedBy:         w.ProcessedBy,
		ParentTransactionId: w.ParentTransactionId,
		TransactionCode:     w.TransactionCode,

		Notes:  w.Notes,
		Amount: w.Amount,

		BalanceBefore: w.BalanceBefore,
		BalanceAfter:  w.BalanceAfter,

		TransactionType: enum.TransactionType(w.TransactionType),
		Status:          enum.TransactionStatus(w.Status),

		ReferenceId:   w.ReferenceId,
		ReferenceType: enum.WalletReferenceType(w.ReferenceType),

		Description:           w.Description,
		ExternalTransactionId: w.ExternalTransactionId,
		Metadata:              w.Metadata,
		PaymentMethod:         enum.WalletTransactionPaymentMethod(w.PaymentMethod),

		CreatedAt:   w.CreatedAt,
		UpdatedAt:   w.UpdatedAt,
		CompletedAt: w.CompletedAt,
		FailedAt:    w.FailedAt,
		ProcessedAt: w.ProcessedAt,

		FailureReason: w.FailureReason,
	}
}

func MapRefundTicketSummary(t *pb.RefundListItem) *wallet.RefundTicketSummary {
	if t == nil {
		return nil
	}

	return &wallet.RefundTicketSummary{
		Id:            t.Id,
		TicketCode:    t.TicketCode,
		Amount:        t.Amount,
		RefundAmount:  t.RefundAmount,
		Reason:        t.Reason,
		UserId:        t.UserId,
		TransactionId: t.TransactionId,
		Status:        enum.RefundTicketStatus(t.Status),
		CreatedAt:     t.CreatedAt,
		UpdatedAt:     t.UpdatedAt,
	}
}

func MapRefundTicketDetail(t *pb.GetRefundByIdResponse) *wallet.RefundTicketDetail {
	if t == nil {
		return nil
	}

	return &wallet.RefundTicketDetail{
		RefundTicketSummary: wallet.RefundTicketSummary{
			Id:            t.Id,
			TicketCode:    t.TicketCode,
			Amount:        t.Amount,
			RefundAmount:  t.RefundAmount,
			Reason:        t.Reason,
			UserId:        t.UserId,
			TransactionId: t.TransactionId,
			Status:        enum.RefundTicketStatus(t.Status),
			CreatedAt:     t.CreatedAt,
			UpdatedAt:     t.UpdatedAt,
		},

		RejectionReason:       t.RejectionReason,
		WalletId:              t.WalletId,
		OriginalTransactionId: t.OriginalTransactionId,
		RefundReason:          enum.RefundReasonType(t.RefundReason),
		RefundMethod:          enum.RefundTicketMethod(t.RefundMethod),
	}
}
