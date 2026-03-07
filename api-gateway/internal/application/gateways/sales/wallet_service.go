package sales

import (
	"api-gateway/internal/domain/models/wallet"
	"context"

	"github.com/google/uuid"
)

type IWalletService interface {
	GetByUserId(ctx context.Context, userId uuid.UUID) (*wallet.WalletResponse, error)
	TopUp(ctx context.Context, userId uuid.UUID, req *wallet.TopUpWalletRequest) (string, error)
	TopUpVnPayReturn(ctx context.Context, queryParams map[string]string) (*wallet.TopUpVnPayReturnResponse, error)

	// Admin only
	UpdateStatus(ctx context.Context, adminId, walletId uuid.UUID, req *wallet.UpdateWalletStatusRequest) error
}
