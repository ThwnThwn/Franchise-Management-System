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
// @Summary      Xem danh sách yêu cầu hoàn tiền của tôi
// @Description  Lấy danh sách các ticket hoàn tiền do chính user đang đăng nhập tạo (có phân trang).
// @Tags         Refunds
// @Produce      json
// @Security     BearerAuth
// @Param        query query    wallet.GetAllRefundsRequest true "Các tham số lọc và phân trang"
// @Success      200   {object} common.PaginationResult[wallet.RefundTicketSummary] "Danh sách ticket"
// @Failure      400   {object} map[string]string "Tham số không hợp lệ"
// @Failure      401   {object} map[string]string "Chưa đăng nhập"
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
// @Summary      Tạo yêu cầu hoàn tiền mới
// @Description  Khách hàng chủ động tạo một yêu cầu hoàn tiền (ticket) cho một giao dịch/đơn hàng.
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     wallet.CreateRefundByCustomerRequest true "Thông tin yêu cầu hoàn tiền"
// @Success      200     {object} map[string]string "Tạo thành công"
// @Failure      400     {object} map[string]string "Dữ liệu đầu vào không hợp lệ"
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
// @Summary      Xem toàn bộ danh sách hoàn tiền
// @Description  Dành cho Staff, Manager, Admin lấy danh sách các ticket hoàn tiền. Có thể truyền thêm `userId` vào query để lọc riêng của một người.
// @Tags         Refunds
// @Produce      json
// @Security     BearerAuth
// @Param        userId query    string false "Lọc theo UUID của người dùng cụ thể (Tùy chọn)"
// @Param        query  query    wallet.GetAllRefundsRequest true "Các tham số lọc và phân trang"
// @Success      200    {object} common.PaginationResult[wallet.RefundTicketSummary] "Danh sách ticket"
// @Failure      400    {object} map[string]string "Tham số không hợp lệ"
// @Failure      401    {object} map[string]string "Chưa đăng nhập"
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
// @Summary      Xem chi tiết một yêu cầu hoàn tiền
// @Description  Lấy thông tin chi tiết của ticket hoàn tiền. Tầng Service sẽ tự check quyền dựa trên ctxUserId.
// @Tags         Refunds
// @Produce      json
// @Security     BearerAuth
// @Param        id  path     string true "UUID của ticket hoàn tiền"
// @Success      200 {object} wallet.RefundTicketDetail
// @Failure      404 {object} map[string]string "Không tìm thấy ticket"
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
// @Summary      Duyệt yêu cầu hoàn tiền
// @Description  (Admin) Chấp nhận và tiến hành hoàn tiền cho khách.
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                      true "UUID của ticket hoàn tiền"
// @Param        request body     wallet.ApproveRefundRequest true "Lý do duyệt"
// @Success      200     {object} map[string]string           "Thành công"
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
// @Summary      Từ chối yêu cầu hoàn tiền
// @Description  (Admin) Từ chối yêu cầu hoàn tiền kèm theo lý do cụ thể.
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                     true "UUID của ticket hoàn tiền"
// @Param        request body     wallet.RejectRefundRequest true "Lý do từ chối"
// @Success      200     {object} map[string]string          "Thành công"
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
// @Summary      Hủy yêu cầu hoàn tiền
// @Description  (Customer/Admin) Hủy bỏ ticket hoàn tiền (thường do khách hàng đổi ý).
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                     true "UUID của ticket hoàn tiền"
// @Param        request body     wallet.CancelRefundRequest true "Lý do hủy"
// @Success      200     {object} map[string]string          "Thành công"
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
// @Summary      Đánh dấu hoàn tất hoàn tiền
// @Description  (Staff/Manager/Admin) Xác nhận quá trình chuyển tiền đã xong, đóng ticket.
// @Tags         Refunds
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string                          true "UUID của ticket hoàn tiền"
// @Param        request body     wallet.SetCompleteRefundRequest true "Ghi chú hoàn tất"
// @Success      200     {object} map[string]string               "Thành công"
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
