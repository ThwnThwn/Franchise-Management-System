package supplier

import (
	"api-gateway/internal/application/constants/errorcode"
	gw "api-gateway/internal/application/gateways/supplier"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type SupplierHandler struct {
	supplierGW gw.ISupplierService
}

func NewSupplierHandler(supplierGW gw.ISupplierService) *SupplierHandler {
	return &SupplierHandler{
		supplierGW: supplierGW,
	}
}

// GetAll
// @Summary      Danh sách nhà cung cấp
// @Tags         Suppliers
// @Security     BearerAuth
// @Param        query query  supplier.GetSuppliersReq true "Tham số lọc và phân trang"
// @Success      200   {object} common.PaginationResult[supplier.SupplierSummary]
// @Router       /suppliers [get]
func (h *SupplierHandler) GetAll(c *gin.Context) error {
	var req supplier.GetSuppliersReq
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.supplierGW.GetAll(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetById
// @Summary      Chi tiết nhà cung cấp
// @Tags         Suppliers
// @Security     BearerAuth
// @Param        id   path   string true "UUID nhà cung cấp"
// @Success      200  {object} supplier.SupplierDetail
// @Router       /suppliers/{id} [get]
func (h *SupplierHandler) GetById(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	res, err := h.supplierGW.GetById(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Create
// @Summary      Tạo mới nhà cung cấp
// @Tags         Suppliers
// @Accept       json
// @Security     BearerAuth
// @Param        request body supplier.CreateSupplierReq true "Thông tin nhà cung cấp"
// @Success      200     {object} supplier.SupplierBaseRes
// @Router       /suppliers [post]
func (h *SupplierHandler) Create(c *gin.Context) error {
	var req supplier.CreateSupplierReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.supplierGW.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Update
// @Summary      Cập nhật nhà cung cấp
// @Tags         Suppliers
// @Accept       json
// @Security     BearerAuth
// @Param        id      path string true "UUID nhà cung cấp"
// @Param        request body supplier.UpdateSupplierReq true "Thông tin cập nhật"
// @Success      200     {object} supplier.SupplierBaseRes
// @Router       /suppliers/{id} [patch]
func (h *SupplierHandler) Update(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req supplier.UpdateSupplierReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.supplierGW.Update(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Delete
// @Summary      Xóa nhà cung cấp
// @Tags         Suppliers
// @Security     BearerAuth
// @Param        id   path   string true "UUID nhà cung cấp"
// @Success      200  {object} supplier.SupplierBaseRes
// @Router       /suppliers/{id} [delete]
func (h *SupplierHandler) Delete(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	res, err := h.supplierGW.Delete(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
