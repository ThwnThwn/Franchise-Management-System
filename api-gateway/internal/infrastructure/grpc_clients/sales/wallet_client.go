package sales

import (
	"api-gateway/config"
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/wallet"
	salesPb "api-gateway/pkg/pb/sales"
	"context"

	"github.com/google/uuid"
)

type walletGrpcClient struct {
	client salesPb.WalletGrpcClient
	cfg    *config.Config
}

// ProvideOrderClient trích xuất Order Client từ Factory
func ProvideWalletClient(f *salesClientFactory, cfg *config.Config) salesGW.IWalletService {
	return &walletGrpcClient{
		client: salesPb.NewWalletGrpcClient(f.conn),
		cfg:    cfg,
	}
}

// GetByUserId implements sales.IWalletService.
func (w *walletGrpcClient) GetByUserId(ctx context.Context, userId uuid.UUID) (*wallet.WalletResponse, error) {
	res, err := w.client.GetWalletByUserId(ctx, &salesPb.GetWalletByUserIdRequest{
		UserId: userId.String(),
	})
	if err != nil {
		return nil, err
	}

	return &wallet.WalletResponse{
		Id:        res.Id,
		UserId:    res.UserId,
		Balance:   res.Balance,
		Status:    enum.WalletStatus(res.Status),
		CreatedAt: res.CreatedAt,
		UpdatedAt: res.UpdatedAt,
	}, nil
}

// TopUp implements sales.IWalletService.
func (w *walletGrpcClient) TopUp(ctx context.Context, userId uuid.UUID, req *wallet.TopUpWalletRequest) (string, error) {
	var returnUrl string
	if req.ReturnUrl != nil {
		returnUrl = *req.ReturnUrl
	} else {
		returnUrl = w.cfg.VNPay.WalletClientReturnUrl
	}

	res, err := w.client.DepositWallet(ctx, &salesPb.DepositWalletRequest{
		UserId:    userId.String(),
		Amount:    req.Amount,
		ReturnUrl: &returnUrl,
	})
	if err != nil {
		return "", err
	}

	return res.PaymentUrl, nil
}

// TopUpVnPayReturn implements sales.IWalletService.
func (w *walletGrpcClient) TopUpVnPayReturn(ctx context.Context, queryParams map[string]string) (*wallet.TopUpVnPayReturnResponse, error) {
	res, err := w.client.DepositVnPayReturn(ctx, &salesPb.DepositVnPayReturnRequest{
		QueryParams: queryParams,
	})
	if err != nil {
		return nil, err
	}

	return &wallet.TopUpVnPayReturnResponse{
		Success:             res.Success,
		WalletTransactionId: res.WalletTransactionId,
		Amount:              res.Amount,
		NewBalance:          res.NewBalance,
	}, nil
}

// UpdateStatus implements sales.IWalletService.
func (w *walletGrpcClient) UpdateStatus(ctx context.Context, adminId uuid.UUID, walletId uuid.UUID,
	req *wallet.UpdateWalletStatusRequest,
) error {
	_, err := w.client.UpdateWalletStatus(ctx, &salesPb.UpdateWalletStatusRequest{
		WalletId:   walletId.String(),
		AdminId:    adminId.String(),
		NewStatus:  salesPb.WalletStatus(req.NewStatus),
		Reason:     req.Reason,
		AdminNotes: req.AdminNotes,
	})

	return err
}
