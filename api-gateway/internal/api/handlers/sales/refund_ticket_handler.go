package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/wallet"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	_ "api-gateway/internal/domain/models/common"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type RefundTicketHandler struct {
	refundGW sales.IRefundTicketService
}

func NewRefundTicketHandler(refundGW sales.IRefundTicketService) *RefundTicketHandler {
	return &RefundTicketHandler{
		refundGW: refundGW,
	}
}

// ----------------------------------------------------------------------
// NHÓM API DÀNH CHO END-USER (Tiền tố: /me/refunds)
// ----------------------------------------------------------------------

// GetMyRefunds
// @Summary      Get my refund requests
// @Description  Retrieve a paginated list of refund tickets created by the currently logged-in user.
// @Tags         Refunds
// @Produce      json
// @Security     BearerAuth
// @Param        query query  wallet.GetAllRefundsRequest true "Filtering and pagination parameters"
// @Success      200   {object} common.PaginationResult[wallet.RefundTicketSummary] "List of refund tickets"
// @Failure      400   {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401   {object} map[string]string "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /me/refunds [get]
func (h *RefundTicketHandler) GetMyRefunds(c *gin.Context) error {
	var req wallet.GetAllRefundsRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	// Luồng End-User: Ép cứng userId lấy từ token vào hàm GetAll
	res, err := h.refundGW.GetAll(ctx, &userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// CreateRefund
// @Summary      Create a new refund request
// @Description  Customer proactively creates a refund request (ticket) for a specific transaction or order.
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     wallet.CreateRefundByCustomerRequest true "Refund request details"
// @Success      200     {object} map[string]string "{"message": "Tạo yêu cầu hoàn tiền thành công"}"
// @Failure      400     {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /me/refunds [post]
func (h *RefundTicketHandler) CreateRefund(c *gin.Context) error {
	var req wallet.CreateRefundByCustomerRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	err := h.refundGW.CreateRefundByCustomer(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Tạo yêu cầu hoàn tiền thành công"})
	return nil
}

// ----------------------------------------------------------------------
// NHÓM API DÀNH CHO QUẢN LÝ (Tiền tố: /refunds)
// ----------------------------------------------------------------------

// GetAllRefunds
// @Summary      Get all refund requests (Admin)
// @Description  Retrieve a list of all refund tickets (Intended for Staff, Manager, Admin). Can optionally pass a `userId` in the query to filter by a specific user.
// @Tags         Refunds
// @Produce      json
// @Security     BearerAuth
// @Param        userId  query    string false "Filter by specific User UUID (Optional)"
// @Param        query   query    wallet.GetAllRefundsRequest true "Filtering and pagination parameters"
// @Success      200     {object} common.PaginationResult[wallet.RefundTicketSummary] "List of refund tickets"
// @Failure      400     {object} map[string]string "Validation Error (Bad Request)"
// @Failure      401     {object} map[string]string "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /refunds [get]
func (h *RefundTicketHandler) GetAllRefunds(c *gin.Context) error {
	// Dùng Struct Nặc Danh để hứng thêm userId từ Query
	var req struct {
		wallet.GetAllRefundsRequest
		UserId *string `form:"userId"`
	}

	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	var targetUserId *uuid.UUID = nil
	if req.UserId != nil && *req.UserId != "" {
		parsedId, parseErr := uuid.Parse(*req.UserId)
		if parseErr != nil {
			return errorcode.ErrInvalidId
		}
		targetUserId = &parsedId
	}

	// Luồng Admin: Truyền con trỏ targetUserId (có thể là nil để lấy toàn bộ)
	res, err := h.refundGW.GetAll(ctx, targetUserId, &req.GetAllRefundsRequest)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ----------------------------------------------------------------------
// NHÓM DÙNG CHUNG (Chi tiết & Trạng thái)
// (Frontend có thể gọi chung qua URL /refunds/:id/... nếu Router setup hợp lý)
// ----------------------------------------------------------------------

// GetRefundById
// @Summary      Get refund ticket details
// @Description  Retrieve detailed information of a specific refund ticket. The service layer handles authorization based on the user's role and token.
// @Tags         Refunds
// @Produce      json
// @Security     BearerAuth
// @Param        id   path     string true "Refund ticket UUID"
// @Success      200  {object} wallet.RefundTicketDetail "Refund ticket details"
// @Failure      401  {object} object "Unauthorized"
// @Failure      404  {object} map[string]string "Refund ticket not found"
// @Failure      500  {object} object "Internal Server Error"
// @Router       /refunds/{id} [get]
func (h *RefundTicketHandler) GetRefundById(c *gin.Context) error {
	ticketIdStr := c.Param("id")
	ticketId, err := uuid.Parse(ticketIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	ctxUserId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	res, err := h.refundGW.GetById(ctx, ctxUserId, ticketId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ApproveRefund
// @Summary      Approve refund request (Admin)
// @Description  Accept and process the refund request for the customer.
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                      true "Refund ticket UUID"
// @Param        request  body     wallet.ApproveRefundRequest true "Approval details/reason"
// @Success      200      {object} map[string]string           "{"message": "Đã duyệt yêu cầu hoàn tiền"}"
// @Failure      400      {object} object "Validation Error or Invalid ID"
// @Failure      401      {object} object "Unauthorized"
// @Failure      500      {object} object "Internal Server Error"
// @Router       /refunds/{id}/approve [patch]
func (h *RefundTicketHandler) ApproveRefund(c *gin.Context) error {
	var req wallet.ApproveRefundRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ticketIdStr := c.Param("id")
	ticketId, err := uuid.Parse(ticketIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	err = h.refundGW.Approve(ctx, ticketId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã duyệt yêu cầu hoàn tiền"})
	return nil
}

// RejectRefund
// @Summary      Reject refund request (Admin)
// @Description  Reject a refund request and provide a specific reason to the customer.
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                     true "Refund ticket UUID"
// @Param        request  body     wallet.RejectRefundRequest true "Rejection reason"
// @Success      200      {object} map[string]string          "{"message": "Đã từ chối yêu cầu hoàn tiền"}"
// @Failure      400      {object} object "Validation Error or Invalid ID"
// @Failure      401      {object} object "Unauthorized"
// @Failure      500      {object} object "Internal Server Error"
// @Router       /refunds/{id}/reject [patch]
func (h *RefundTicketHandler) RejectRefund(c *gin.Context) error {
	var req wallet.RejectRefundRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ticketIdStr := c.Param("id")
	ticketId, err := uuid.Parse(ticketIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	err = h.refundGW.Reject(ctx, ticketId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã từ chối yêu cầu hoàn tiền"})
	return nil
}

// CancelRefund
// @Summary      Cancel refund request
// @Description  Cancel an active refund ticket (usually performed by the customer if they change their mind).
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                     true "Refund ticket UUID"
// @Param        request  body     wallet.CancelRefundRequest true "Cancellation reason"
// @Success      200      {object} map[string]string          "{"message": "Đã hủy yêu cầu hoàn tiền"}"
// @Failure      400      {object} object "Validation Error or Invalid ID"
// @Failure      401      {object} object "Unauthorized"
// @Failure      500      {object} object "Internal Server Error"
// @Router       /refunds/{id}/cancel [patch]
func (h *RefundTicketHandler) CancelRefund(c *gin.Context) error {
	var req wallet.CancelRefundRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ticketIdStr := c.Param("id")
	ticketId, err := uuid.Parse(ticketIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	err = h.refundGW.Cancel(ctx, ticketId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã hủy yêu cầu hoàn tiền"})
	return nil
}

// CompleteRefund
// @Summary      Mark refund as complete (Admin)
// @Description  Confirm that the money transfer process is successful and close the refund ticket.
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                          true "Refund ticket UUID"
// @Param        request  body     wallet.SetCompleteRefundRequest true "Completion notes"
// @Success      200      {object} map[string]string               "{"message": "Đã đánh dấu hoàn tất yêu cầu hoàn tiền"}"
// @Failure      400      {object} object "Validation Error or Invalid ID"
// @Failure      401      {object} object "Unauthorized"
// @Failure      500      {object} object "Internal Server Error"
// @Router       /refunds/{id}/complete [patch]
func (h *RefundTicketHandler) CompleteRefund(c *gin.Context) error {
	var req wallet.SetCompleteRefundRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ticketIdStr := c.Param("id")
	ticketId, err := uuid.Parse(ticketIdStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	err = h.refundGW.SetComplete(ctx, ticketId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã đánh dấu hoàn tất yêu cầu hoàn tiền"})
	return nil
}
