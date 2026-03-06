package mapper

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/withdrawal"
	pb "api-gateway/pkg/pb/sales"
)

func MapWithdrawalSummary(r *pb.WithdrawalSummary) *withdrawal.WithdrawalSummary {
	if r == nil {
		return nil
	}

	return &withdrawal.WithdrawalSummary{
		Id:                  r.Id,
		RequestCode:         r.RequestCode,
		UserId:              r.UserId,
		Amount:              r.Amount,
		FeeAmount:           r.FeeAmount,
		BankCode:            r.BankCode,
		BankName:            r.BankName,
		AccountName:         r.AccountName,
		MaskedAccountNumber: r.MaskedAccountNumber,
		Status:              enum.WithdrawalStatus(r.Status),
		CreatedAt:           r.CreatedAt,
	}
}

func MapWithdrawalDetail(r *pb.WithdrawalDetailResponse) *withdrawal.WithdrawalDetailRes {
	if r == nil {
		return nil
	}

	return &withdrawal.WithdrawalDetailRes{
		WithdrawalSummary: withdrawal.WithdrawalSummary{
			Id:                  r.Id,
			RequestCode:         r.RequestCode,
			UserId:              r.UserId,
			Amount:              r.Amount,
			FeeAmount:           r.FeeAmount,
			BankCode:            r.BankCode,
			BankName:            r.BankName,
			AccountName:         r.AccountName,
			MaskedAccountNumber: r.MaskedAccountNumber,
			Status:              enum.WithdrawalStatus(r.Status),
			CreatedAt:           r.CreatedAt,
		},

		RawAccountNumber:   r.RawAccountNumber,
		Notes:              r.Notes,
		InternalNotes:      r.InternalNotes,
		ApprovalNotes:      r.ApprovalNotes,
		RejectionReason:    r.RejectionReason,
		CancellationReason: r.CancellationReason,
		ReceiptImageUrl:    r.ReceiptImageUrl,
		TransactionId:      r.TransactionId,
		UpdatedAt:          r.UpdatedAt,
	}
}
