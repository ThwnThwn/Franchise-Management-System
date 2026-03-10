package withdrawal

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
)

// --- NHÓM END-USER ---

type CreateWithdrawalReq struct {
	Amount        string  `json:"amount" binding:"required"`
	BankCode      *string `json:"bankCode,omitempty"`
	BankName      string  `json:"bankName" binding:"required"`
	AccountName   string  `json:"accountName" binding:"required"`
	AccountNumber string  `json:"accountNumber" binding:"required"`
	Notes         string  `json:"notes"`
}

type GetMyWithdrawalsReq struct {
	common.PaginationQuery
	Status enum.WithdrawalStatus `form:"status"`
}

type CancelMyWithdrawalReq struct {
	CancellationReason string `json:"cancellationReason" binding:"required"`
}

// --- NHÓM ADMIN ---

type GetAllWithdrawalsReq struct {
	common.PaginationQuery
	UserId *string               `form:"userId"`
	Status enum.WithdrawalStatus `form:"status"`
}

type GetWithdrawalByIdReq struct {
	RequestId string `form:"requestId" binding:"required"`
}

type ApproveWithdrawalReq struct {
	ApprovalNotes string `json:"approvalNotes"`
}

type RejectWithdrawalReq struct {
	RejectionReason string `json:"rejectionReason" binding:"required"`
}

type CancelWithdrawalReq struct {
	CancellationReason string `json:"cancellationReason" binding:"required"`
}

type CompleteWithdrawalReq struct {
	BankTransactionId string `json:"-"`
	ReceiptFile       []byte `json:"-"` // Xử lý bằng MultipartForm trong handler
}
