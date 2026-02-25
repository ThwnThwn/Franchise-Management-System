package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/constants/roleslug"
	"api-gateway/internal/domain/models/shift"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ShiftTemplateHandler struct {
	shiftGW franchise.IShiftTemplateService
}

func NewShiftTemplateHandler(shiftGW franchise.IShiftTemplateService) *ShiftTemplateHandler {
	return &ShiftTemplateHandler{shiftGW: shiftGW}
}

// GetAll
// @Summary      Lấy danh sách mẫu ca làm việc
// @Description  Truy xuất toàn bộ danh sách các mẫu ca làm (Sáng, Chiều, Tối...) theo Store.
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Param        query query shift.GetShiftTemplatesParams false "Bộ lọc tìm kiếm mẫu ca"
// @Success      200 {array} shift.ShiftTemplateResponse "Danh sách mẫu ca"
// @Router       /shift-templates [get]
func (h *ShiftTemplateHandler) GetAll(c *gin.Context) {
	var params shift.GetShiftTemplatesParams
	if err := c.ShouldBindQuery(&params); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	res, err := h.shiftGW.GetAll(c.Request.Context(), &params)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// GetByID
// @Summary      Chi tiết mẫu ca làm việc
// @Description  Lấy thông tin chi tiết cấu hình của một mẫu ca làm việc dựa trên ID.
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Param        id path string true "ID mẫu ca"
// @Success      200 {object} shift.ShiftTemplateResponse
// @Router       /shift-templates/{id} [get]
func (h *ShiftTemplateHandler) GetByID(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	res, err := h.shiftGW.GetById(c.Request.Context(), id)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// Create
// @Summary      Tạo mẫu ca làm mới
// @Description  Định nghĩa một khung giờ làm việc mẫu (mã code, tên, giờ bắt đầu/kết thúc).
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Param        body body shift.CreateShiftTemplateRequest true "Thông tin mẫu ca"
// @Router       /shift-templates [post]
func (h *ShiftTemplateHandler) Create(c *gin.Context) {
	var req shift.CreateShiftTemplateRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	roleSlug, err := contextutils.GetRoleSlug(c)
	if err != nil {
		c.Error(err)
		return
	}

	if roleSlug == string(roleslug.Manager) {
		storeId, err := contextutils.GetStoreId(c)
		if err != nil {
			c.Error(err)
			return
		}
		req.StoreID = stringutils.ToPtr(storeId.String())
	} else {
		req.StoreID = nil
	}

	err = h.shiftGW.Create(c.Request.Context(), &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusCreated, gin.H{})
}

// Update
// @Summary      Cập nhật mẫu ca làm việc
// @Description  Chỉnh sửa cấu hình khung giờ hoặc tên của mẫu ca làm việc hiện có.
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Param        id path string true "ID mẫu ca"
// @Param        body body shift.UpdateShiftTemplateRequest true "Dữ liệu cập nhật"
// @Router       /shift-templates/{id} [patch]
func (h *ShiftTemplateHandler) Update(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req shift.UpdateShiftTemplateRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	err = h.shiftGW.Update(c.Request.Context(), id, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// Delete
// @Summary      Xóa mẫu ca làm việc
// @Description  Gỡ bỏ hoàn toàn một mẫu ca làm khỏi hệ thống.
// @Tags         Shifts: Template
// @Security     BearerAuth
// @Param        id path string true "ID mẫu ca"
// @Router       /shift-templates/{id} [delete]
func (h *ShiftTemplateHandler) Delete(c *gin.Context) {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	err = h.shiftGW.Delete(c.Request.Context(), id)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
