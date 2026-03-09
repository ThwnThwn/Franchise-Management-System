package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/withdrawal"
	"api-gateway/pkg/utils/validation"
	"io"
	"net/http"

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
// @Summary      Tạo yêu cầu rút tiền mới
// @Description  Khách hàng tạo yêu cầu rút tiền từ ví vào tài khoản ngân hàng. Tiền sẽ bị tạm trừ ngay lập tức.
// @Description  Dùng api https://api.vietqr.io/v2/banks để lấy bank code và name cho khách chọn
// @Description  Vẫn cho 1 ô nhập tên ngân hàng nếu api lỗi hoặc khách ko thấy trong danh sách
// @Description  Trường hợp khách nhập tên ngân hàng thủ công thì ko cần code
// @Tags         Withdrawals
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body      withdrawal.CreateWithdrawalReq true "Thông tin yêu cầu rút tiền"
// @Success      200     {object}  withdrawal.WithdrawalBaseRes "Tạo thành công"
// @Failure      400     {object}  map[string]string "Dữ liệu không hợp lệ"
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
// @Summary      Xem danh sách yêu cầu rút tiền của tôi
// @Description  Lấy lịch sử rút tiền của chính người dùng đang đăng nhập (có phân trang).
// @Tags         Withdrawals
// @Produce      json
// @Security     BearerAuth
// @Param        query   query     withdrawal.GetMyWithdrawalsReq true "Tham số lọc và phân trang"
// @Success      200     {object}  common.PaginationResult[withdrawal.WithdrawalSummary] "Danh sách yêu cầu"
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
// @Summary      Khách hàng tự hủy yêu cầu rút tiền
// @Description  Người dùng tự hủy lệnh khi chưa được Admin xử lý (Trạng thái PENDING). Tiền hoàn về ví.
// @Tags         Withdrawals
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path      string                         true "UUID của yêu cầu rút tiền"
// @Param        request body      withdrawal.CancelMyWithdrawalReq true "Lý do hủy"
// @Success      200     {object}  map[string]string "Hủy thành công"
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
// @Summary      Xem toàn bộ danh sách yêu cầu rút tiền (Admin)
// @Description  Dành cho quản trị viên xem và lọc danh sách rút tiền của toàn bộ người dùng.
// @Tags         Withdrawals
// @Produce      json
// @Security     BearerAuth
// @Param        query   query     withdrawal.GetAllWithdrawalsReq true "Tham số lọc của Admin"
// @Success      200     {object}  common.PaginationResult[withdrawal.WithdrawalSummary] "Danh sách yêu cầu"
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
// @Summary      Xem chi tiết yêu cầu rút tiền
// @Description  Lấy đầy đủ thông tin một yêu cầu rút tiền bao gồm số tài khoản đầy đủ (Raw) và lịch sử xử lý.
// @Tags         Withdrawals
// @Produce      json
// @Security     BearerAuth
// @Param        id      path      string true "UUID của yêu cầu rút tiền"
// @Success      200     {object}  withdrawal.WithdrawalDetailRes
// @Failure      404     {object}  map[string]string "Không tìm thấy"
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
// @Summary      Duyệt yêu cầu rút tiền
// @Description  (Admin) Xác nhận yêu cầu hợp lệ và chuẩn bị chuyển tiền. Trạng thái chuyển sang APPROVED.
// @Tags         Withdrawals
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path      string                         true "UUID của yêu cầu rút tiền"
// @Param        request body      withdrawal.ApproveWithdrawalReq true "Ghi chú duyệt"
// @Success      200     {object}  map[string]string "Thành công"
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
// @Summary      Admin từ chối yêu cầu (Chỉ dành cho lệnh PENDING)
// @Description  Dùng để từ chối lệnh khi đang PENDING. Hệ thống tự hoàn tiền vào ví.
// @Tags         Withdrawals
// @Security     BearerAuth
// @Param        id      path      string                        true "ID yêu cầu"
// @Param        request body      withdrawal.RejectWithdrawalReq true "Lý do từ chối"
// @Success      200     {object}  map[string]string
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
// @Summary      Admin hủy yêu cầu (Dành cho hầu hết trạng thái)
// @Description  Dùng để hủy lệnh ở bất kỳ giai đoạn nào (Pending, Approved, Processing) TRỪ KHI đã thành công/hủy rồi. Hệ thống tự hoàn tiền vào ví.
// @Tags         Withdrawals
// @Security     BearerAuth
// @Param        id      path      string                        true "ID yêu cầu"
// @Param        request body      withdrawal.CancelWithdrawalReq true "Lý do hủy từ Admin"
// @Success      200     {object}  map[string]string
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
// @Summary      Hoàn tất yêu cầu rút tiền (Kèm hóa đơn)
// @Description  (Admin) Xác nhận đã chuyển khoản thành công. Phải upload ảnh bill (receiptFile) để minh chứng.
// @Tags         Withdrawals
// @Accept       mpfd
// @Produce      json
// @Security     BearerAuth
// @Param        id           path      string  true  "UUID của yêu cầu rút tiền"
// @Param        receiptFile  formData  file    true  "Ảnh chụp biên lai chuyển khoản (Bill)"
// @Success      200          {object}  map[string]string "Hoàn tất"
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
		ReceiptFile: fileBytes,
	}

	ctx := c.Request.Context()

	err = h.withdrawalGW.Complete(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã hoàn tất yêu cầu rút tiền và cập nhật bằng chứng chuyển khoản."})
	return nil
}
