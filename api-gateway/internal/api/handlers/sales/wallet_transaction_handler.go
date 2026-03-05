package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/wallet"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type WalletTransactionHandler struct {
	transGW sales.IWalletTransactionService
}

func NewWalletTransactionHandler(transGW sales.IWalletTransactionService) *WalletTransactionHandler {
	return &WalletTransactionHandler{
		transGW: transGW,
	}
}

// ----------------------------------------------------------------------
// NHÓM API DÀNH CHO END-USER (Tiền tố: /me/wallet)
// ----------------------------------------------------------------------

// GetMyWalletTransactions
// @Summary      Xem lịch sử giao dịch ví của tôi
// @Description  Lấy danh sách các giao dịch ví (nạp tiền, trừ tiền, hoàn tiền...) của người dùng đang đăng nhập có phân trang.
// @Tags         Wallet Transactions
// @Produce      json
// @Security     BearerAuth
// @Param        query query    wallet.GetAllWalletTransactionsRequest true "Các tham số lọc và phân trang"
// @Success      200   {object} common.PaginationResult[wallet.WalletTransactionResponse] "Danh sách giao dịch"
// @Failure      400   {object} map[string]string "Tham số không hợp lệ"
// @Failure      401   {object} map[string]string "Chưa đăng nhập"
// @Router       /me/wallet/transactions [get]
func (h *WalletTransactionHandler) GetMyWalletTransactions(c *gin.Context) error {
	var req wallet.GetAllWalletTransactionsRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	res, err := h.transGW.GetByUserId(ctx, userId, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ----------------------------------------------------------------------
// NHÓM API DÀNH CHO QUẢN LÝ (Tiền tố: /wallets/transactions)
// ----------------------------------------------------------------------

// GetAllTransactions
// @Summary      Xem toàn bộ giao dịch ví hệ thống
// @Description  Dành cho Staff, Manager, Admin lấy danh sách giao dịch. Có thể truyền thêm `userId` vào query để lọc riêng giao dịch của một người.
// @Tags         Wallet Transactions
// @Produce      json
// @Security     BearerAuth
// @Param        userId query    string false "Lọc theo UUID của người dùng cụ thể (Tùy chọn)"
// @Param        query  query    wallet.GetAllWalletTransactionsRequest true "Các tham số lọc và phân trang"
// @Success      200    {object} common.PaginationResult[wallet.WalletTransactionResponse] "Danh sách giao dịch"
// @Failure      400    {object} map[string]string "Tham số không hợp lệ"
// @Failure      401    {object} map[string]string "Chưa đăng nhập"
// @Failure      403    {object} map[string]string "Không có quyền thao tác"
// @Router       /wallets/transactions [get]
func (h *WalletTransactionHandler) GetAllTransactions(c *gin.Context) error {
	var req struct {
		wallet.GetAllWalletTransactionsRequest
		UserId *string `form:"userId"`
	}

	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	ctxUserId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	// Biến hứng kết quả chung cho cả 2 luồng
	var res common.PaginationResult[*wallet.WalletTransactionResponse]
	var apiErr error

	if req.UserId != nil && *req.UserId != "" {
		targetUserId, parseErr := uuid.Parse(*req.UserId)
		if parseErr != nil {
			return errorcode.ErrInvalidId
		}
		res, apiErr = h.transGW.GetByUserId(ctx, ctxUserId, targetUserId, &req.GetAllWalletTransactionsRequest)
	} else {
		res, apiErr = h.transGW.GetAll(ctx, ctxUserId, &req.GetAllWalletTransactionsRequest)
	}

	if apiErr != nil {
		return apiErr
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ----------------------------------------------------------------------
// NHÓM TRA CỨU CHI TIẾT GIAO DỊCH (Dùng chung dựa theo ctxUserId)
// ----------------------------------------------------------------------

// GetTransactionById
// @Summary      Lấy chi tiết giao dịch bằng ID
// @Description  Xem chi tiết một giao dịch ví thông qua UUID của giao dịch. Tầng service sẽ tự kiểm tra xem `ctxUserId` có quyền xem ID này hay không.
// @Tags         Wallet Transactions
// @Produce      json
// @Security     BearerAuth
// @Param        id  path     string true "UUID của giao dịch"
// @Success      200 {object} wallet.WalletTransactionResponse
// @Failure      404 {object} map[string]string "Không tìm thấy giao dịch"
// @Router       /wallets/transactions/{id} [get]
func (h *WalletTransactionHandler) GetTransactionById(c *gin.Context) error {
	transIdStr := c.Param("id")
	transId, err := uuid.Parse(transIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	ctxUserId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	res, err := h.transGW.GetById(ctx, ctxUserId, transId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetTransactionByCode
// @Summary      Lấy chi tiết giao dịch bằng Mã giao dịch
// @Description  Xem chi tiết một giao dịch ví thông qua mã code (VD: WT-12345).
// @Tags         Wallet Transactions
// @Produce      json
// @Security     BearerAuth
// @Param        code path    string true "Mã giao dịch (Code)"
// @Success      200  {object} wallet.WalletTransactionResponse
// @Failure      404  {object} map[string]string "Không tìm thấy giao dịch"
// @Router       /wallets/transactions/code/{code} [get]
func (h *WalletTransactionHandler) GetTransactionByCode(c *gin.Context) error {
	code := c.Param("code")
	if code == "" {
		return errorcode.NewBadRequest("Mã giao dịch không được để trống")
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	ctxUserId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	res, err := h.transGW.GetByCode(ctx, ctxUserId, code)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
