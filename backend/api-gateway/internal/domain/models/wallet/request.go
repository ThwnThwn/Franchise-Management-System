package wallet

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
)

// Wallet
type TopUpWalletRequest struct {
	Amount    float64 `json:"amount"`
	ReturnUrl *string `json:"returnUrl"`
}

type GetAllWalletTransactionsRequest struct {
	common.PaginationQuery
}

type UpdateWalletStatusRequest struct {
	NewStatus  enum.WalletStatus `json:"newStatus"`
	Reason     string            `json:"reason"`
	AdminNotes string            `json:"adminNotes"`
}

// Refund
type GetAllRefundsRequest struct {
	common.PaginationQuery
	Status *enum.RefundTicketStatus `form:"status"`
}

type CreateRefundByCustomerRequest struct {
	Amount string `json:"amount"`
	Reason string `json:"reason"`
}

type ApproveRefundRequest struct {
	ApprovalNotes string `json:"approvalNotes"`
}

type RejectRefundRequest struct {
	RejectNotes string `json:"rejectNotes"`
}

type CancelRefundRequest struct {
	CancellationReason string `json:"cancellationReason"`
}

type SetCompleteRefundRequest struct {
	CompleteNotes *string `json:"completeNotes"`
}
