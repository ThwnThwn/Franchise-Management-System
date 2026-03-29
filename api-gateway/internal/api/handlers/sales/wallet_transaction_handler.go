package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/wallet"
	"api-gateway/pkg/utils/contextutils"
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
// @Summary      Get my wallet transaction history
// @Description  Retrieve a paginated list of wallet transactions (top-ups, deductions, refunds, etc.) for the currently logged-in user.
// @Tags         Wallet Transactions
// @Produce      json
// @Security     BearerAuth
// @Param        query query    wallet.GetAllWalletTransactionsRequest true "Filtering and pagination parameters"
// @Success      200   {object} common.PaginationResult[wallet.WalletTransactionResponse] "List of transactions"
// @Failure      400   {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401   {object} map[string]string "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /me/wallet/transactions [get]
func (h *WalletTransactionHandler) GetMyWalletTransactions(c *gin.Context) error {
	var req wallet.GetAllWalletTransactionsRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

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
// @Summary      Get all system wallet transactions (Admin)
// @Description  Intended for Staff, Manager, or Admin to retrieve a list of all wallet transactions. Can optionally pass a `userId` in the query to filter transactions for a specific user.
// @Tags         Wallet Transactions
// @Produce      json
// @Security     BearerAuth
// @Param        userId query    string false "Filter by specific User UUID (Optional)"
// @Param        query  query    wallet.GetAllWalletTransactionsRequest true "Filtering and pagination parameters"
// @Success      200    {object} common.PaginationResult[wallet.WalletTransactionResponse] "List of transactions"
// @Failure      400    {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401    {object} map[string]string "Unauthorized"
// @Failure      403    {object} map[string]string "Forbidden (Insufficient permissions)"
// @Failure      500    {object} object "Internal Server Error"
// @Router       /wallets/transactions [get]
func (h *WalletTransactionHandler) GetAllTransactions(c *gin.Context) error {
	var req struct {
		wallet.GetAllWalletTransactionsRequest
		UserId *string `form:"userId"`
	}

	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

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
// @Summary      Get transaction details by ID
// @Description  View details of a specific wallet transaction using its UUID. The service layer verifies if the requesting user has permission to view this transaction.
// @Tags         Wallet Transactions
// @Produce      json
// @Security     BearerAuth
// @Param        id   path     string true "Transaction UUID"
// @Success      200  {object} wallet.WalletTransactionResponse "Transaction details"
// @Failure      401  {object} map[string]string "Unauthorized"
// @Failure      404  {object} map[string]string "Transaction not found"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /wallets/transactions/{id} [get]
func (h *WalletTransactionHandler) GetTransactionById(c *gin.Context) error {
	transIdStr := c.Param("id")
	transId, err := uuid.Parse(transIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

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
// @Summary      Get transaction details by Code
// @Description  View details of a specific wallet transaction using its unique code (e.g., WT-12345).
// @Tags         Wallet Transactions
// @Produce      json
// @Security     BearerAuth
// @Param        code path     string true "Transaction Code"
// @Success      200  {object} wallet.WalletTransactionResponse "Transaction details"
// @Failure      400  {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401  {object} map[string]string "Unauthorized"
// @Failure      404  {object} map[string]string "Transaction not found"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /wallets/transactions/code/{code} [get]
func (h *WalletTransactionHandler) GetTransactionByCode(c *gin.Context) error {
	code := c.Param("code")
	if code == "" {
		return errorcode.NewBadRequest("Mã giao dịch không được để trống")
	}

	ctx := c.Request.Context()

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
