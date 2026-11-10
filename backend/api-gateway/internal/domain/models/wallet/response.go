package wallet

import (
	"api-gateway/internal/domain/constants/enum"
)

// Wallet
type WalletResponse struct {
	Id        string            `json:"id"`
	UserId    string            `json:"userId"`
	Balance   string            `json:"balance"`
	Status    enum.WalletStatus `json:"status"`
	CreatedAt string            `json:"createdAt"`
	UpdatedAt string            `json:"updatedAt"`
}

type TopUpVnPayReturnResponse struct {
	Success             bool    `json:"success"`
	WalletTransactionId string  `json:"walletTransactionId"`
	Amount              float64 `json:"amount"`
	NewBalance          float64 `json:"newBalance"`
}

type WalletTransactionResponse struct {
	Id                    string                              `json:"id"`
	WalletId              string                              `json:"walletId"`
	UserId                string                              `json:"userId"`
	OrderId               string                              `json:"orderId"`
	InitiatedBy           string                              `json:"initiatedBy"`
	ProcessedBy           string                              `json:"processedBy"`
	ParentTransactionId   string                              `json:"parentTransactionId"`
	TransactionCode       string                              `json:"transactionCode"`
	Notes                 string                              `json:"notes"`
	Amount                string                              `json:"amount"`
	BalanceBefore         string                              `json:"balanceBefore"`
	BalanceAfter          string                              `json:"balanceAfter"`
	TransactionType       enum.TransactionType                `json:"transactionType"`
	Status                enum.TransactionStatus              `json:"status"`
	ReferenceId           string                              `json:"referenceId"`
	ReferenceType         enum.WalletReferenceType            `json:"referenceType"`
	Description           string                              `json:"description"`
	ExternalTransactionId string                              `json:"externalTransactionId"`
	Metadata              string                              `json:"metadata"`
	PaymentMethod         enum.WalletTransactionPaymentMethod `json:"paymentMethod"`
	CreatedAt             string                              `json:"createdAt"`
	UpdatedAt             string                              `json:"updatedAt"`
	CompletedAt           string                              `json:"completedAt"`
	FailedAt              string                              `json:"failedAt"`
	ProcessedAt           string                              `json:"processedAt"`
	FailureReason         string                              `json:"failureReason"`
}

// Refund
type RefundTicketSummary struct {
	Id            string                  `json:"id"`
	TicketCode    string                  `json:"ticketCode"`
	Amount        string                  `json:"amount"`
	RefundAmount  string                  `json:"refundAmount"`
	Reason        string                  `json:"reason"`
	UserId        string                  `json:"userId"`
	TransactionId string                  `json:"transactionId"`
	Status        enum.RefundTicketStatus `json:"status"`
	CreatedAt     string                  `json:"createdAt"`
	UpdatedAt     string                  `json:"updatedAt"`
}

type RefundTicketDetail struct {
	RefundTicketSummary

	RejectionReason       string                  `json:"rejectionReason"`
	WalletId              string                  `json:"walletId"`
	OriginalTransactionId string                  `json:"originalTransactionId"`
	RefundReason          enum.RefundReasonType   `json:"refundReason"`
	RefundMethod          enum.RefundTicketMethod `json:"refundMethod"`
}
