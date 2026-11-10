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
// @Summary      Get all suppliers
// @Description  Retrieve a paginated list of all suppliers in the system.
// @Tags         Suppliers
// @Produce      json
// @Security     BearerAuth
// @Param        query query  supplier.GetSuppliersReq true "Filtering and pagination parameters"
// @Success      200   {object} common.PaginationResult[supplier.SupplierSummary] "List of suppliers"
// @Failure      400   {object} object "Validation Error (Bad Request)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
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
// @Summary      Get supplier details
// @Description  Retrieve detailed information of a specific supplier by their UUID.
// @Tags         Suppliers
// @Produce      json
// @Security     BearerAuth
// @Param        id    path   string true "Supplier UUID"
// @Success      200   {object} supplier.SupplierDetail "Detailed supplier information"
// @Failure      400   {object} object "Invalid UUID format"
// @Failure      401   {object} object "Unauthorized"
// @Failure      404   {object} object "Supplier Not Found"
// @Failure      500   {object} object "Internal Server Error"
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
// @Summary      Create a new supplier
// @Description  Add a new supplier to the system.
// @Tags         Suppliers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     supplier.CreateSupplierReq true "Supplier information"
// @Success      200     {object} supplier.SupplierBaseRes "Successfully created"
// @Failure      400     {object} object "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
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
// @Summary      Update a supplier
// @Description  Modify the details of an existing supplier.
// @Tags         Suppliers
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string true "Supplier UUID"
// @Param        request body     supplier.UpdateSupplierReq true "Updated information"
// @Success      200     {object} supplier.SupplierBaseRes "Successfully updated"
// @Failure      400     {object} object "Validation Error or Invalid ID"
// @Failure      401     {object} object "Unauthorized"
// @Failure      404     {object} object "Supplier Not Found"
// @Failure      500     {object} object "Internal Server Error"
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
// @Summary      Delete a supplier
// @Description  Remove a supplier from the system.
// @Tags         Suppliers
// @Produce      json
// @Security     BearerAuth
// @Param        id    path     string true "Supplier UUID"
// @Success      200   {object} supplier.SupplierBaseRes "Successfully deleted"
// @Failure      400   {object} object "Invalid UUID format"
// @Failure      401   {object} object "Unauthorized"
// @Failure      404   {object} object "Supplier Not Found"
// @Failure      500   {object} object "Internal Server Error"
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
