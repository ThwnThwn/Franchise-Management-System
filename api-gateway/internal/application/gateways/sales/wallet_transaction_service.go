package sales

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/wallet"
	"context"

	"github.com/google/uuid"
)

type IWalletTransactionService interface {
	GetAll(ctx context.Context, ctxUserId uuid.UUID, req *wallet.GetAllWalletTransactionsRequest) (
		common.PaginationResult[*wallet.WalletTransactionResponse], error)
	GetByUserId(ctx context.Context, ctxUserId, userIdToSearch uuid.UUID, req *wallet.GetAllWalletTransactionsRequest) (
		common.PaginationResult[*wallet.WalletTransactionResponse], error)

	GetById(ctx context.Context, ctxUserId, transId uuid.UUID) (*wallet.WalletTransactionResponse, error)
	GetByCode(ctx context.Context, ctxUserId uuid.UUID, code string) (*wallet.WalletTransactionResponse, error)
}
