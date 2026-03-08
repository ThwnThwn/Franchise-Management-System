package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftSwapHandler struct {
	swapGW franchise.IShiftSwapService
}

func NewShiftSwapHandler(swapGW franchise.IShiftSwapService) *ShiftSwapHandler {
	return &ShiftSwapHandler{swapGW: swapGW}
}

// RequestSwap
// @Summary      Tạo yêu cầu đổi ca (A gửi cho B)
// @Description  Nhân viên A chọn một ca của mình và yêu cầu đổi với nhân viên B.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path string true "Assignment ID (Ca của người gửi)"
// @Param        body body shift.RequestSwapRequest true "Dữ liệu yêu cầu đổi ca"
// @Router       /shifts/{id}/swaps [post]
func (h *ShiftSwapHandler) RequestSwap(c *gin.Context) error {
	assignmentId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.RequestSwapRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	if err := h.swapGW.RequestSwap(ctx, assignmentId, userId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusCreated, gin.H{"message": "Đã gửi yêu cầu đổi ca"})
	return nil
}

// RespondSwap
// @Summary      Nhân viên phản hồi yêu cầu đổi ca (B phản hồi cho A)
// @Description  Nhân viên B nhận được yêu cầu và chọn Chấp nhận hoặc Từ chối.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path string true "Swap Request ID"
// @Param        body body shift.RespondSwapRequest true "Phản hồi của nhân viên nhận yêu cầu"
// @Router       /shift-swaps/{id}/responses [patch]
func (h *ShiftSwapHandler) RespondSwap(c *gin.Context) error {
	swapId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.RespondSwapRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	if err := h.swapGW.RespondSwap(ctx, swapId, userId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã ghi nhận phản hồi đổi ca"})
	return nil
}

// ApproveSwap
// @Summary      Manager duyệt yêu cầu đổi ca cuối cùng
// @Description  Sau khi 2 nhân viên đồng ý, Manager cần duyệt cuối để chính thức đổi ca.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id   path string true "Swap Request ID"
// @Param        body body shift.ApproveSwapRequest true "Kết quả duyệt của Manager"
// @Router       /shift-swaps/{id}/approvals [patch]
func (h *ShiftSwapHandler) ApproveSwap(c *gin.Context) error {
	swapId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req shift.ApproveSwapRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	managerId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	if err := h.swapGW.ApproveSwap(ctx, swapId, managerId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã hoàn tất duyệt đổi ca"})
	return nil
}

// CancelSwap
// @Summary      Tự hủy yêu cầu đổi ca đã gửi
// @Description  Nhân viên gửi yêu cầu có thể tự hủy nếu yêu cầu chưa được phê duyệt.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Produce      json
// @Param        id path string true "Swap Request ID"
// @Router       /shift-swaps/{id}/cancel [patch]
func (h *ShiftSwapHandler) CancelSwap(c *gin.Context) error {
	swapId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	if err := h.swapGW.CancelSwap(ctx, swapId, userId); err != nil {
		return err
	}

	c.JSON(http.StatusNoContent, nil)
	return nil
}

// Release
// @Summary      Nhả ca về Chợ Ca (Shift Pool)
// @Description  Nhân viên trả lại ca làm đã nhận để người khác có thể đăng ký.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Produce      json
// @Param        id path string true "Assignment ID"
// @Router       /shifts/{id}/releases [patch]
func (h *ShiftSwapHandler) Release(c *gin.Context) error {
	assignmentId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	if err := h.swapGW.Release(ctx, assignmentId, userId); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Đã nhả ca thành công"})
	return nil
}

// GetAll
// @Summary      Lấy danh sách yêu cầu đổi ca
// @Description  Lấy toàn bộ các yêu cầu đổi ca.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Produce      json
// @Param        query  query     shift.GetAllSwapsParams  false  "Filter params"
// @Router       /shift-swaps [get]
func (h *ShiftSwapHandler) GetAll(c *gin.Context) error {
	var params shift.GetAllSwapsParams
	if err := c.ShouldBindQuery(&params); err != nil {
		return validation.TranslateValidationError(err)
	}

	contextutils.ApplyStoreBoundaries(c, &params.StoreId)

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	data, err := h.swapGW.GetAll(ctx, &params)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, data)
	return nil
}

// GetMySwaps
// @Summary      Lấy danh sách yêu cầu đổi ca của tôi
// @Description  Lấy toàn bộ các yêu cầu đổi ca mà tôi là người gửi hoặc người nhận.
// @Tags         Shifts: Swaps
// @Security     BearerAuth
// @Produce      json
// @Router       /me/shift-swaps [get]
func (h *ShiftSwapHandler) GetMySwaps(c *gin.Context) error {
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	data, err := h.swapGW.GetMySwaps(ctx, userId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, data)
	return nil
}
