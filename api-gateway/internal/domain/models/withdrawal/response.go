package withdrawal

import "api-gateway/internal/domain/constants/enum"

type WithdrawalBaseRes struct {
	Id      string `json:"id"`
	Message string `json:"message"`
}

type WithdrawalSummary struct {
	Id                  string                `json:"id"`
	RequestCode         string                `json:"requestCode"`
	UserId              string                `json:"userId"`
	Amount              float64               `json:"amount"`
	FeeAmount           float64               `json:"feeAmount"`
	BankCode            *string               `json:"bankCode,omitempty"`
	BankName            string                `json:"bankName"`
	AccountName         string                `json:"accountName"`
	MaskedAccountNumber string                `json:"maskedAccountNumber"`
	Status              enum.WithdrawalStatus `json:"status"`
	CreatedAt           string                `json:"createdAt"`
}

type WithdrawalDetailRes struct {
	WithdrawalSummary

	RawAccountNumber   string `json:"rawAccountNumber"`
	Notes              string `json:"notes"`
	InternalNotes      string `json:"internalNotes"`
	ApprovalNotes      string `json:"approvalNotes"`
	RejectionReason    string `json:"rejectionReason"`
	CancellationReason string `json:"cancellationReason"`
	ReceiptImageUrl    string `json:"receiptImageUrl"`
	TransactionId      string `json:"transactionId"`
	UpdatedAt          string `json:"updatedAt"`
}
