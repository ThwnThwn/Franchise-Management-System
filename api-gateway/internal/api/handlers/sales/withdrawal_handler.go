package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/withdrawal"
	"api-gateway/pkg/utils/validation"
	"io"
	"net/http"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type WithdrawalHandler struct {
	withdrawalGW sales.IWithdrawalService
}

func NewWithdrawalHandler(withdrawalGW sales.IWithdrawalService) *WithdrawalHandler {
	return &WithdrawalHandler{
		withdrawalGW: withdrawalGW,
	}
}

// ----------------------------------------------------------------------
// NHÓM API DÀNH CHO END-USER (Tiền tố: /me/withdrawals)
// ----------------------------------------------------------------------

// CreateWithdrawal
// @Summary      Create a new withdrawal request
// @Description  Customer creates a request to withdraw funds from their wallet to a bank account. The requested amount is temporarily deducted immediately.
// @Description  Use the API https://api.vietqr.io/v2/banks to fetch bank codes and names for the customer to select.
// @Description  Provide a manual input field for the bank name in case the API fails or the bank is not listed.
// @Description  If the customer enters the bank name manually, the bank code is not required.
// @Tags         Withdrawals
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     withdrawal.CreateWithdrawalReq true "Withdrawal request details"
// @Success      200     {object} withdrawal.WithdrawalBaseRes "Successfully created"
// @Failure      400     {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /me/withdrawals [post]
func (h *WithdrawalHandler) CreateWithdrawal(c *gin.Context) error {
	var req withdrawal.CreateWithdrawalReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err := h.withdrawalGW.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// GetMyWithdrawals
// @Summary      Get my withdrawal requests
// @Description  Retrieve the paginated withdrawal history for the currently logged-in user.
// @Tags         Withdrawals
// @Produce      json
// @Security     BearerAuth
// @Param        query   query    withdrawal.GetMyWithdrawalsReq true "Filtering and pagination parameters"
// @Success      200     {object} common.PaginationResult[withdrawal.WithdrawalSummary] "List of user's withdrawal requests"
// @Failure      400     {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /me/withdrawals [get]
func (h *WithdrawalHandler) GetMyWithdrawals(c *gin.Context) error {
	var req withdrawal.GetMyWithdrawalsReq
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.withdrawalGW.GetMyWithdrawals(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// CancelMyWithdrawal
// @Summary      Cancel my withdrawal request
// @Description  User cancels their own withdrawal request before it is processed by an Admin (PENDING status). Funds are refunded back to the wallet.
// @Tags         Withdrawals
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                           true "Withdrawal request UUID"
// @Param        request body     withdrawal.CancelMyWithdrawalReq true "Cancellation reason"
// @Success      200     {object} map[string]string "{"message": "Đã hủy yêu cầu rút tiền thành công và hoàn tiền vào ví."}"
// @Failure      400     {object} map[string]string "Validation Error or Invalid ID"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /me/withdrawals/{id}/cancel [patch]
func (h *WithdrawalHandler) CancelMyWithdrawal(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req withdrawal.CancelMyWithdrawalReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err = h.withdrawalGW.CancelMyWithdrawal(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã hủy yêu cầu rút tiền thành công và hoàn tiền vào ví."})
	return nil
}

// ----------------------------------------------------------------------
// NHÓM API DÀNH CHO ADMIN/KẾ TOÁN (Tiền tố: /withdrawals)
// ----------------------------------------------------------------------

// GetAllWithdrawals
// @Summary      Get all withdrawal requests (Admin)
// @Description  Intended for administrators and accountants to view and filter the withdrawal requests of all users across the system.
// @Tags         Withdrawals
// @Produce      json
// @Security     BearerAuth
// @Param        query   query    withdrawal.GetAllWithdrawalsReq true "Filtering and pagination parameters"
// @Success      200     {object} common.PaginationResult[withdrawal.WithdrawalSummary] "List of system withdrawal requests"
// @Failure      400     {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /withdrawals [get]
func (h *WithdrawalHandler) GetAllWithdrawals(c *gin.Context) error {
	var req withdrawal.GetAllWithdrawalsReq
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.withdrawalGW.GetAll(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetWithdrawalById
// @Summary      Get withdrawal request details
// @Description  Retrieve full details of a withdrawal request, including the raw bank account number and the processing history.
// @Tags         Withdrawals
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string true "Withdrawal request UUID"
// @Success      200     {object} withdrawal.WithdrawalDetailRes "Detailed withdrawal information"
// @Failure      400     {object} map[string]string "Invalid UUID format"
// @Failure      401     {object} object "Unauthorized"
// @Failure      404     {object} map[string]string "Withdrawal request not found"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /withdrawals/{id} [get]
func (h *WithdrawalHandler) GetWithdrawalById(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	res, err := h.withdrawalGW.GetById(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ApproveWithdrawal
// @Summary      Approve withdrawal request (Admin)
// @Description  Confirm that the request is valid and prepare for the fund transfer. The status changes to APPROVED.
// @Tags         Withdrawals
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                          true "Withdrawal request UUID"
// @Param        request body     withdrawal.ApproveWithdrawalReq true "Approval notes"
// @Success      200     {object} map[string]string "{"message": "Đã duyệt yêu cầu rút tiền"}"
// @Failure      400     {object} map[string]string "Validation Error or Invalid ID"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /withdrawals/{id}/approve [patch]
func (h *WithdrawalHandler) ApproveWithdrawal(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req withdrawal.ApproveWithdrawalReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err = h.withdrawalGW.Approve(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã duyệt yêu cầu rút tiền"})
	return nil
}

// RejectWithdrawal
// @Summary      Reject withdrawal request (Admin)
// @Description  Reject a PENDING withdrawal request. The system automatically refunds the deducted amount back to the user's wallet.
// @Tags         Withdrawals
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                         true "Withdrawal request UUID"
// @Param        request body     withdrawal.RejectWithdrawalReq true "Rejection reason"
// @Success      200     {object} map[string]string "{"message": "Đã từ chối yêu cầu rút tiền và hoàn tiền vào ví."}"
// @Failure      400     {object} map[string]string "Validation Error or Invalid ID"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /withdrawals/{id}/reject [patch]
func (h *WithdrawalHandler) RejectWithdrawal(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req withdrawal.RejectWithdrawalReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err = h.withdrawalGW.Reject(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã từ chối yêu cầu rút tiền và hoàn tiền vào ví."})
	return nil
}

// CancelWithdrawal
// @Summary      Cancel withdrawal request (Admin)
// @Description  Cancel a withdrawal request at any stage (Pending, Approved, Processing) UNLESS it is already completed or cancelled. The system automatically refunds the amount to the user's wallet.
// @Tags         Withdrawals
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                         true "Withdrawal request UUID"
// @Param        request body     withdrawal.CancelWithdrawalReq true "Cancellation reason from Admin"
// @Success      200     {object} map[string]string "{"message": "Admin đã hủy lệnh và hoàn tiền thành công."}"
// @Failure      400     {object} map[string]string "Validation Error or Invalid ID"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /withdrawals/{id}/cancel [patch]
func (h *WithdrawalHandler) CancelWithdrawal(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req withdrawal.CancelWithdrawalReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err = h.withdrawalGW.Cancel(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Admin đã hủy lệnh và hoàn tiền thành công."})
	return nil
}

// CompleteWithdrawal
// @Summary      Complete withdrawal request (Admin)
// @Description  Confirm that the bank transfer was successful. A receipt image (receiptFile) must be uploaded as proof.
// @Tags         Withdrawals
// @Accept       multipart/form-data
// @Produce      json
// @Security     BearerAuth
// @Param        id                path     string  true "Withdrawal request UUID"
// @Param        receiptFile       formData file    true "Transfer receipt image (Bill)"
// @Param        bankTransactionId formData string  true "Bank transaction ID"
// @Success      200               {object} map[string]string "{"message": "Đã hoàn tất yêu cầu rút tiền và cập nhật bằng chứng chuyển khoản."}"
// @Failure      400               {object} map[string]string "Missing required file or transaction ID"
// @Failure      401               {object} object "Unauthorized"
// @Failure      500               {object} object "Internal Server Error"
// @Router       /withdrawals/{id}/complete [patch]
func (h *WithdrawalHandler) CompleteWithdrawal(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	file, err := c.FormFile("receiptFile")
	if err != nil {
		return errorcode.NewBadRequest("Cần hình ảnh xác minh việc chuyển khoản")
	}

	bankTxId := c.PostForm("bankTransactionId")
	if strings.TrimSpace(bankTxId) == "" {
		return errorcode.NewBadRequest("Vui lòng nhập Mã giao dịch ngân hàng")
	}

	openedFile, err := file.Open()
	if err != nil {
		return err
	}
	defer openedFile.Close()

	fileBytes, err := io.ReadAll(openedFile)
	if err != nil {
		return err
	}

	req := withdrawal.CompleteWithdrawalReq{
		BankTransactionId: bankTxId,
		ReceiptFile:       fileBytes,
	}

	ctx := c.Request.Context()

	err = h.withdrawalGW.Complete(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã hoàn tất yêu cầu rút tiền và cập nhật bằng chứng chuyển khoản."})
	return nil
}
