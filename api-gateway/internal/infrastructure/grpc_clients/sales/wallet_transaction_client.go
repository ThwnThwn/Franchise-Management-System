package sales

import (
	salesGW "api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/wallet"
	"api-gateway/pkg/mapper"
	salesPb "api-gateway/pkg/pb/sales"
	"context"

	"github.com/google/uuid"
)

type wallettransactionGrpcClient struct {
	client salesPb.WalletTransactionGrpcClient
}

// ProvideOrderClient trích xuất Order Client từ Factory
func ProvideWalletTransactionClient(f *salesClientFactory) salesGW.IWalletTransactionService {
	return &wallettransactionGrpcClient{
		client: salesPb.NewWalletTransactionGrpcClient(f.conn),
	}
}

// GetAll implements sales.IWalletTransactionService.
func (w *wallettransactionGrpcClient) GetAll(ctx context.Context, ctxUserId uuid.UUID,
	req *wallet.GetAllWalletTransactionsRequest,
) (common.PaginationResult[*wallet.WalletTransactionResponse], error) {
	res, err := w.client.GetAll(ctx, &salesPb.GetAllWalletTransactionsRequest{
		LoggedUserId: ctxUserId.String(),
		PageIndex:    req.Page,
		PageSize:     req.PageSize,
	})
	if err != nil {
		return common.PaginationResult[*wallet.WalletTransactionResponse]{}, err
	}

	items := make([]*wallet.WalletTransactionResponse, 0, len(res.Transactions))
	for _, item := range res.Transactions {
		items = append(items, mapper.MapWalletTransactionPbToModel(item))
	}

	return common.PaginationResult[*wallet.WalletTransactionResponse]{
		Items:      items,
		TotalCount: res.Meta.TotalCount,
		TotalPages: res.Meta.TotalPages,
	}, nil
}

// GetById implements sales.IWalletTransactionService.
func (w *wallettransactionGrpcClient) GetById(ctx context.Context, ctxUserId uuid.UUID, transId uuid.UUID) (
	*wallet.WalletTransactionResponse, error,
) {
	res, err := w.client.GetById(ctx, &salesPb.GetWalletTransactionByIdRequest{
		TransactionId: transId.String(),
		LoggedUserId:  ctxUserId.String(),
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapWalletTransactionPbToModel(res), nil
}

// GetByCode implements sales.IWalletTransactionService.
func (w *wallettransactionGrpcClient) GetByCode(ctx context.Context, ctxUserId uuid.UUID, code string) (
	*wallet.WalletTransactionResponse, error,
) {
	res, err := w.client.GetByCode(ctx, &salesPb.GetWalletTransactionByCodeRequest{
		TransactionCode: code,
		LoggedUserId:    ctxUserId.String(),
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapWalletTransactionPbToModel(res), nil
}

// GetByUserId implements sales.IWalletTransactionService.
func (w *wallettransactionGrpcClient) GetByUserId(ctx context.Context, ctxUserId uuid.UUID, userIdToSearch uuid.UUID,
	req *wallet.GetAllWalletTransactionsRequest,
) (common.PaginationResult[*wallet.WalletTransactionResponse], error) {
	res, err := w.client.GetByUserId(ctx, &salesPb.GetWalletTransactionsByUserIdRequest{
		UserIdToSearch: userIdToSearch.String(),
		LoggedUserId:   ctxUserId.String(),
		PageIndex:      req.Page,
		PageSize:       req.PageSize,
	})
	if err != nil {
		return common.PaginationResult[*wallet.WalletTransactionResponse]{}, err
	}

	items := make([]*wallet.WalletTransactionResponse, 0, len(res.Transactions))
	for _, item := range res.Transactions {
		items = append(items, mapper.MapWalletTransactionPbToModel(item))
	}

	return common.PaginationResult[*wallet.WalletTransactionResponse]{
		Items:      items,
		TotalCount: res.Meta.TotalCount,
		TotalPages: res.Meta.TotalPages,
	}, nil
}
